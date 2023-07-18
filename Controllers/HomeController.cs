using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EmsWeb.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration;
using static NativeLinuxMethods;

using System.Text.Json;
using System.Text.Json.Serialization;

using System.Text.Json;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Threading;



namespace EmsWeb.Controllers
{
    public class HomeController : Controller
    {
        //test
        private static EmsConfiguration EmsConfiguration { get; set; }
        public IConfiguration Configuration { get; set; }
        private readonly ILogger<HomeController> _logger;
        //---------------------------------------------------------------------
        public HomeController(ILogger<HomeController> logger)
        {
            Configuration = new ConfigurationBuilder()
           .AddJsonFile("EmsConfigs.json")
           .Build();

            _logger = logger;
        }
        //---------------------------------------------------------------------
        public IActionResult Index()
        {
            return View();
        }
        //---------------------------------------------------------------------
        public IActionResult Privacy()
        {
            return View();
        }
        //---------------------------------------------------------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        //---------------------------------------------------------------------
        [HttpGet]
        public string GetSettings() // Send data to service
        {
            return Startup._data;
        }
        //---------------------------------------------------------------------
        [HttpPost]
        public string GetEmsLogs(string _log) // Get EMS logs
        {
            Console.WriteLine("=> Ems log is : " + _log);
            Startup.EmsLogs = _log;

            return "This is test data";
        }
        //---------------------------------------------------------------------
        [HttpPost]
        public string UpdatePageData()
        {
            return Startup.EmsLogs;
        }
        //---------------------------------------------------------------------
        [HttpGet]
        public ActionResult ActiveOnGrid(string _data)
        {
            using (var writer = new StreamWriter("EmsConfigs.json"))
            {
                writer.WriteLine("{");
                writer.WriteLine("\"EmsConfigs\" : \"" + _data + "\"");

                writer.WriteLine("}");
            }

            Startup._data = _data; // Write data from view to my public variable

            Console.WriteLine("On Grid is sent");

            DoForIPC("1");

            return Json("ok value changed to " + Startup._data); // Send data from view to my public variable as Json packet
        }
        //---------------------------------------------------------------------
        [HttpGet]
        public ActionResult ActiveOffGrid(string _data)
        {
            using (var writer = new StreamWriter("EmsConfigs.json"))
            {
                writer.WriteLine("{");
                writer.WriteLine("\"EmsConfigs\" : \"" + _data + "\"");

                writer.WriteLine("}");
            }

            Startup._data = _data; // Write data from view to my public variable

            Console.WriteLine("Off Grid is sent");

            DoForIPC("0");
            return Json("ok value changed to " + Startup._data); // Send data from view to my public variable as Json packet
        }
        //---------------------------------------------------------------------
        private void DoForIPC(string _in)
        {
            switch (_in)
            {
                case "0":
                    {

                        try
                        {
                            var dir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DeviceConfig");

                            var configfiles = Directory.GetFiles("/opt/ems/DeviceConfig/", "*.json");
                            foreach (var configfile in configfiles)
                            {
                                Console.WriteLine($"Found config file {configfile}");

                                using (StreamReader jsonReader = new StreamReader(configfile))
                                {
                                    if (configfile.EndsWith("Ems.json"))
                                    {
                                        EmsConfiguration = JsonConvert.DeserializeObject<EmsConfiguration>(System.IO.File.ReadAllText(configfile));
                                    }
                                }
                                Thread.Sleep(2000);
                                using (StreamReader jsonReader = new StreamReader(configfile))
                                {
                                    if (configfile.EndsWith("Ems.json"))
                                    {
                                        EmsConfiguration.IsOffgrid = true;
                                        UpdateConfigFile("Ems.json", EmsConfiguration);
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading device config file: {ex.Message}");
                        }

                        break;
                    }
                case "1":
                    {
                        Console.WriteLine("RECEIVED FROM WEB ====>>> OnGrid");

                        try
                        {
                            var dir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DeviceConfig");

                            var configfiles = Directory.GetFiles("/opt/ems/DeviceConfig/", "*.json");
                            foreach (var configfile in configfiles)
                            {
                                Console.WriteLine($"Found config file {configfile}");

                                using (StreamReader jsonReader = new StreamReader(configfile))
                                {
                                    if (configfile.EndsWith("Ems.json"))
                                    {
                                        EmsConfiguration = JsonConvert.DeserializeObject<EmsConfiguration>(System.IO.File.ReadAllText(configfile));
                                    }
                                }

                                Thread.Sleep(2000);
                                using (StreamReader jsonReader = new StreamReader(configfile))
                                {
                                    if (configfile.EndsWith("Ems.json"))
                                    {
                                        // EmsConfiguration = JsonConvert.DeserializeObject<EmsConfiguration>(File.ReadAllText(configfile));
                                        EmsConfiguration.IsOffgrid = false;
                                        UpdateConfigFile("Ems.json", EmsConfiguration);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading device config file: {ex.Message}");
                        }

                        break;
                    }
            }

            Console.WriteLine("==================>>>>>>>>>>>>>>>>> Restart IPC");

            Thread.Sleep(1000);

            RebootIPC();
        }
        //---------------------------------------------------------------------
        public static void RebootIPC()
        {
            var isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (isLinux)
            {
                //Int32 ret = reboot(LINUX_REBOOT_MAGIC1, LINUX_REBOOT_MAGIC2, LINUX_REBOOT_CMD_POWER_OFF, IntPtr.Zero);
                Int32 ret = reboot(LINUX_REBOOT_CMD_RESTART);

                // `reboot(LINUX_REBOOT_CMD_POWER_OFF)` never returns if it's successful, so if it returns 0 then that's weird, we should treat it as an error condition instead of success:
                if (ret == 0) throw new InvalidOperationException("reboot(LINUX_REBOOT_CMD_RESTART) returned 0.");

                //  reboot(LINUX_REBOOT_CMD_RESTART, IntPtr.Zero);

                // ..otherwise we expect it to return -1 in the event of failure, so any other value is exceptional:
                if (ret != -1) throw new InvalidOperationException("Unexpected reboot() return value: " + ret);

                // At this point, ret == -1, which means check `errno`!
                // `errno` is accessed via Marshal.GetLastWin32Error(), even on non-Win32 platforms and especially even on Linux

                Int32 errno = Marshal.GetLastWin32Error();
                switch (errno)
                {
                    case EPERM:
                        throw new UnauthorizedAccessException("You do not have permission to call reboot()");

                    case EINVAL:
                        throw new ArgumentException("Bad magic numbers (stray cosmic-ray?)");

                    case EFAULT:
                    default:
                        throw new InvalidOperationException("Could not call reboot():" + errno.ToString());
                }
            }
        }
        //---------------------------------------------------------------------
        private static void UpdateConfigFile(string fileName, object configuration)
        {
            // var dir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DeviceConfig/");
            var path = "/opt/ems/DeviceConfig/" + fileName;

            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(configuration, Formatting.Indented));

            Console.WriteLine($"Updated file: {path}");
        }
        //---------------------------------------------------------------------
    }
















































    public class EmsConfiguration
    {
        public int TelemetryInterval { get; set; } = 300;
        public bool LogPowers { get; set; } = false;
        public bool FanControl { get; set; } = true;
        public bool MeasureRoomTemperature { get; set; } = true;
        public float RoomTemperatureOffset { get; set; } = 0.0f;
        public bool IgnoreFireAlarm { get; set; } = false;
        public bool IgnoreDoorAlarm { get; set; } = false;
        public int FanOnTemperature { get; set; }
        public int FanOffTemperature { get; set; }
        public bool FanRelaisNormallyClosed { get; set; }

        public bool AcLoadRelayControl { get; set; } = true;

        //Voltage limits
        //Below 2.95V minCellVoltage: Battery controller Error, BMS will open relais by itself
        public float MinCellVoltageError { get; set; }                  //e.g. 3.10V. Error situation, relais will be switched off
        public float MinCellVoltageWarning { get; set; }                //e.g. 3.15V. Warning level, starts EmergencyCharge/TrickleCharger
        public float MinCellVoltageStopDischarge { get; set; }          //e.g. 3.20V. Warning amd EmergencyCharge removed again above this level, below this level all discharging is blocked
        public float MinCellVoltageStopDischargeEnd { get; set; }       //e.g. 3.30V. If all discharging was blocked, it is allowed again (upto MaxDischargePowerAtLowBattery) above this level
        public float MinCellVoltageReducedDischargePower { get; set; }  //e.g. 3.40V. Limit discharge power to MaxDischargePowerAtLowBattery below this level
        //public float MinCellVoltageReducedDischargeEnd { get; set; }    //This parameter has become obsolete since we linear decrease the max discharge power

        //public float MaxCellVoltageReducedChargingEnd { get; set; }      //This parameter has become obsolete since we linear decrease the max charge power
        public float MaxCellVoltageReducedCharging { get; set; }         //e.g. 4.05V. Limit charge power to MaxChargePowerAtHighBattery above this level
        public float MaxCellVoltageStopChargingEnd { get; set; }         //e.g. 4.09V. If external charge requests were blocked, they are allowed again below this level at MaxChargePowerAtHighBattery
        public float MaxCellVoltageStopCharging { get; set; }            //e.g. 4.13V. External charge requests are blocked, only trickle charge
        public float MaxCellVoltageStopTrickleCharge { get; set; }      //e.g. 4.15V. Warning removed below this level, above this level no charging at all
        public float MaxCellVoltageWarning { get; set; }                //e.g. 4.17V. Warning level
        public float MaxCellVoltageError { get; set; }                  //e.g. 4.20V. Error situation, relais will be switched off
        //Above 4.3V maxcellvoltage: Battery controller Error, BMS will open relais by itself

        //Temperature limits
        public float MinCellTemperatureWarningEnd { get; set; }
        public float MinCellTemperatureWarning { get; set; }
        public float MaxCellTemperatureWarningEnd { get; set; }
        public float MaxCellTemperatureWarning { get; set; }
        public float MaxCellTemperature { get; set; }

        public float ReeferHeatBelow { get; set; }
        public float ReeferStopHeatAbove { get; set; }
        public float ReeferCoolAbove { get; set; }
        public float ReeferStopCoolBelow { get; set; }
        public float ReeferStartVentilationDeltaT { get; set; } = 10.0f; //Start internal fan when indoor temp diffs more than DeltaT from min/max CellTemp 

        public bool PreCharge { get; set; } = true;
        public bool ForceBatteryDisconnectAtStartup { get; set; } = true;
        public bool UsePvOptimisation { get; set; } = true;
        public bool UseSchedule { get; set; } = true;
        public bool UseZeroGrid { get; set; } = false;
        public int SoCminPvOptimisation { get; set; } = 50;    //Minimum SoC during PV optimisation. Below this value charging on grid will take place.
        public int SoCminSchedule { get; set; } = 10;          //Minimum SoC during Schedule. Below this value charging on grid will take place.
        public int SoChyst { get; set; } = 5;
        public float ZeroGridPowerThreshold { get; set; } = 0.2f; //Below this threshold (+ or -), the ZeroGrid algoritm will not request a power

        public bool TrickleCharge { get; set; } = false;
        public int TrickleChargeMinOnSeconds { get; set; } = 300;
        public int TrickleChargeMinOffSeconds { get; set; } = 60;
        public bool HasStandAloneTrickleCharger { get; set; } = false;
        public float TrickleChargeMaxCabinetVoltage { get; set; }
        public float TrickleChargePower { get; set; }

        public float PeakShaveStartPower { get; set; } //Above this grid power the converter starts immeadiately with PeakShavePower and will start to regulate Grid to GridTargetPower
        public float GridTargetPower { get; set; } //Used for Peakshave and Automatic charging. Peakshave power will gradually be reduced to this power on the Grid and Automatic charge will only take wat is possible without going over the target for long times
        public float PeakShaveStopPower { get; set; }
        public float AutomaticChargePower { get; set; } //Is used for emergency charge (SoC to low during schedule) and for "charging on grid" during PV optimization when SoC to low
        public float PeakShavePower { get; set; }
        public int MinimumPeakShaveTime { get; set; }
        //public float DischargeImmeadiateStopPower { get; set; } //This parameter has become obsolete with a new peakshave algorithm
        //public float MaxDischargePowerAtLowBattery { get; set; } //This parameter has become obsolete since we linear decrease the max discharge power
        public float MaxChargePowerAtHighBattery { get; set; }
        public float MaxChargePowerAtPv { get; set; } = 5.0f; //Max charge power used during PV optimization, default 5.0 kW
        public string MainMeterPeripheralId { get; set; }

        public bool AlwaysUseAllConverters { get; set; }
        public float ConverterDeltaVFactor { get; set; }

        public float ManualOverrulePowerKw { get; set; } = 0.0f; //Used for testing to give a power setpoint. Typically done to test the battery capacity 

        public int GridCapacityL1Ampere { get; set; }
        public int GridCapacityL2Ampere { get; set; }
        public int GridCapacityL3Ampere { get; set; }
        public bool IsOffgrid { get; set; } = false;
        public bool HasChargerSwitch { get; set; } = false; //If a Charger switch is connected to Deditec, the charging can be switched on/off in Offgrid mode
        public float MinSocForOutputStart { get; set; } = 40.0f;
        public bool ChargingStation { get; set; } = false;
    }
}

//-----------------------------------------------------------------------------
// OnGrid = 0
// OffGrid = 1
// Start Converter = 2
// Stop Converter = 3
//-----------------------------------------------------------------------------
function SetOnGrid()
{
    alert("OnGrid is sent");

    $.get("Home/ActiveOnGrid?_data=0"); 
}
//-----------------------------------------------------------------------------
function SetOffGrid()
{
    alert("OffGrid is sent");

    $.get("Home/ActiveOffGrid?_data=1");
}
//-----------------------------------------------------------------------------
function SetStartConverter()
{
    alert("Start Converter is sent");

    $.get("Home/ActiveStartConverter?_data=2");
}
//-----------------------------------------------------------------------------
function SetStopConverter()
{
    alert("Stop Converter is sent");

    $.get("Home/ActiveStopConverter?_data=3");
}
//-----------------------------------------------------------------------------

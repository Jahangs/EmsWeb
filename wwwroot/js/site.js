

//-----------------------------------------------------------------------------
$(document).ready(function () {
    document.getElementById('tbl_off_grid').hidden = true;
    document.getElementById('tbl_on_grid').hidden = false;
});
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
function SetOnGrid() {

    document.getElementById('tbl_off_grid').hidden = true;
    document.getElementById('tbl_on_grid').hidden = false;

    //$.get("Home/ActiveOnGrid?_data=1"); 
}
//-----------------------------------------------------------------------------
function SetOffGrid() {
    document.getElementById('tbl_on_grid').hidden = true;
    document.getElementById('tbl_off_grid').hidden = false;

    //$.get("Home/ActiveOffGrid?_data=0");
}
//-----------------------------------------------------------------------------
// Use a timer here to read 
setInterval(UpdatePageData, 2000);

function UpdatePageData() {
    $.ajax({
        url: "Home/UpdatePageData",
        type: "POST",
        data: {
        },
        success: function (data) {
            //  alert(data);
            //  $('#txt_system_status').text(data);

            document.getElementById('txt_system_status').innerHTML = data;


        }
    });
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

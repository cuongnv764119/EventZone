$("#AddnewUser").submit(function (event) {
    $("#alertPanelAddUser").hide();
    //@*Validate form.*@
    $("#alertPanelAddUser").empty();
    var isValidForm = $("#AddnewUser").validate().form();

    //@*Form is invalid.*@
    if (!isValidForm) {
        $("#addUser-Password").val("");
        $("#addUser-ConfirmPassword").val("");
        return false;
    }
    //hash password
    $("#addUser-Password").val($.md5($("#addUser-Password").val()));
    $("#addUser-ConfirmPassword").val($.md5($("#addUser-ConfirmPassword").val()));
    return true;
});

function OnSucess(data) {
    if (data.state === 1) {
        $("#addUser-Password").val("");
        $("#addUser-ConfirmPassword").val("");

        $("#add-new-user-modal").modal("hide");
        var r = confirm("You created an account! Do you want take a look?")
        if (r == true) {
            window.open("/User/Index?userID="+data.userID, '_blank');
        }
    } else {
        $("#addUser-Password").val("");
        $("#addUser-ConfirmPassword").val("");
        $("#alertPanelAddUser").empty();
        $("#alertPanelAddUser").append("<p>" + data.message + "</p>");
        $("#alertPanelAddUser").show();
    }
}
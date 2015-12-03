$("#ChangePasswordForm").submit(function (event) {
    $("#alertPanelChangePassword").hide();
    //@*Validate form.*@
    $("#alertPanelChangePassword").empty();
    var isValidForm = $("#ChangePasswordForm").validate().form();

    //@*Form is invalid.*@
    if (!isValidForm) {
        $("#chg-password-old-password").val("");
        $("#chg-password-new-password").val("");
        $("#chg-confirm-password").val("");
        return false;
    }
    //hash password
    $("#chg-password-old-password").val($.md5($("#chg-password-old-password").val()));
    $("#chg-password-new-password").val($.md5($("#chg-password-new-password").val()));
    $("#chg-confirm-password").val($.md5($("#chg-confirm-password").val()));

    return true;
});

function OnChangePasswordRequestSucceeded(data) {
    if (data.state === 1) {
        $("#chg-password-old-password").val("");
        $("#chg-password-new-password").val("");
        $("#chg-confirm-password").val("");
        location.reload();
        $("#change-password-modal").modal("toggle");
        $(".modal-backdrop").remove();
    } else {
        $("#chg-password-old-password").val("");
        $("#chg-password-new-password").val("");
        $("#chg-confirm-password").val("");
        $("#alertPanelChangePassword").empty();
        $("#alertPanelChangePassword").append("<p>" + data.message + "</p>");
        $("#alertPanelChangePassword").show();
    }
}
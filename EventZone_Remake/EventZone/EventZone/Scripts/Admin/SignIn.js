$("#formAdminSignIn").submit(function (event) {
    //@*Validate form.*@
    var isValidForm = $("#formAdminSignIn").validate().form();
    //@*Form is invalid.*@
    if (!isValidForm) {

        //@*Prevent this form from being submitted*@
        event.preventDefault();
        $("#admin-signin-password").val("");
        return false;
    }
    $("#admin-signin-password").val($.md5($("#admin-signin-password").val()));
    return true;
});
$("#i_quen_mk").click(function () {
    $("#myModal2").modal("toggle");
    $("#ForgotPasswordInfo").empty();
});
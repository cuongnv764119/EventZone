$("#ConfirmGoogleForm").submit(function (event) {

    var isValidForm = $("#ConfirmGoogleForm").validate().form();

    //@*Form is invalid.*@
    if (!isValidForm) {
        $("#gg-pass").val("");
        $("#confirm-gg-pass").val("");
        return false;
    }
    //hash password
    $("#gg-pass").val($.md5($("#gg-pass").val()));
    $("#confirm-gg-pass").val($.md5($("#confirm-gg-pass").val()));
    return true;
});
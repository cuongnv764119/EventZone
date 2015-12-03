$("#SignUpForm").submit(function (event) {
    $("#alertPanelSignUp").hide();
    //@*Validate form.*@
    $("#alertPanelSignUp").empty();
    var isValidForm = $("#SignUpForm").validate().form();

    //@*Form is invalid.*@
    if (!isValidForm) {
        $("#signup-Password").val("");
        $("#signup-ConfirmPassword").val("");
        return false;
    }
    if (!($('#policy').is(':checked'))) {
        $('#policyError').show();
        return false;
    }
    //hash password
    $("#signup-Password").val($.md5($("#signup-Password").val()));
    $("#signup-ConfirmPassword").val($.md5($("#signup-ConfirmPassword").val()));
    return true;
});

function OnSignUpRequestSucceeded(data) {
    if (data.state === 1) {
        $("#signup-Password").val("");
        $("#signup-ConfirmPassword").val("");
        location.reload();
    } else {
        $("#signup-Password").val("");
        $("#signup-ConfirmPassword").val("");
        $("#alertPanelSignUp").empty();
        $("#alertPanelSignUp").append("<p>" + data.message + "</p>");
        $("#alertPanelSignUp").show();
    }
    console.log("This event is fired when a request has been sent to server successfully");
    console.log(data);
}
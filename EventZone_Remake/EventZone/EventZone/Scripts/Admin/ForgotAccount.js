$("#ForgotPasswordForm").submit(function(event) {
    //@*Validate form.*@
    var isValidForm = $("#ForgotPasswordForm").validate().form();
    //@*Form is invalid.*@
    if (!isValidForm) {

        //@*Prevent this form from being submitted*@
        event.preventDefault();

        return false;
    }

    return true;
});

function OnForgotPasswordRequestSucceeded(data) {
    $("#ForgotPasswordInfo").empty();
    $("#ForgotPasswordInfo").append("<p>" + data.message + "</p>");
    $("#ForgotPasswordInfo").show();
};
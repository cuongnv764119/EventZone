
$("#ChangeUserEmailForm").submit(function (event) {
    //@*Validate form.*@
    var isValidForm = $("#ChangeUserEmailForm").validate().form();
    //@*Form is invalid.*@
    if (!isValidForm) {
        //@*Prevent this form from being submitted*@
        event.preventDefault();
        $("#new-email").val("");
        return false;
    }
    return true;
});

function OnChangeEmailSucess(data) {
    if (data.state == 1) {
        $("#change-email-modal").modal("hide");
        $("#ChangeEmailError").empty();
        $("#email-display-" + data.userID).empty();
        $("#email-display-" + data.userID).append("<p>Email: "+data.newEmail+"</p>");
        alert("Change email success!");
    } else {
        $("#ChangeEmailError").empty();
        $("#ChangeEmailError").append("<p>" + data.message + "</p>");
        $("#ChangeEmailError").show();
    }
};
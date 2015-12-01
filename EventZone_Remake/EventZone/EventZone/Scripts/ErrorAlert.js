function errorShow(title, message) {
    $("#error-title").text(title)
    $("#error-message").text(message)
    $("#error-modal").modal('show')
}
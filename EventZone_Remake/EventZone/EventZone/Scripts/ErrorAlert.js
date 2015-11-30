function errorShow(title, message) {
    $("#error-title").text(title)
    console.log(title)
    $("#error-message").text(message)
    console.log(message)
    $("#error-modal").modal('show')
}
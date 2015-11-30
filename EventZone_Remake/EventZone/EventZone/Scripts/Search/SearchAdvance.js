document.getElementById("AdvDate").style.visibility = "hidden";

function showDate() {
    document.getElementById("AdvDate").style.visibility = "visible";
}

function hideDate() {
    document.getElementById("AdvDate").style.visibility = "hidden";
}

var Advautocomplete;

function initAutocomplete() {
    // Create the autocomplete object, restricting the search to geographical
    // location types.
    Advautocomplete = new google.maps.places.Autocomplete(
    (document.getElementById("AdvSearchLocation")),
    { types: [] });

    // When the user selects an address from the dropdown, populate the address
    // fields in the form.
    Advautocomplete.addListener("place_changed", getCoordinates);
}

// [START region_fillform]
function getCoordinates() {
    // Get the place details from the autocomplete object.
    var place = Advautocomplete.getPlace();
    document.getElementById("AdvLocationLng").value = place.geometry.location.lng();
    document.getElementById("AdvLocationLat").value = place.geometry.location.lat();
}

// Geolocation get current location for lookAround
var shareLocation = function() {
    navigator.geolocation.getCurrentPosition(function(position) {
        $("#id-lookaround-btn").attr("href", $("#id-lookaround-btn").attr("href") + "longitude=" + position.coords.longitude + "&latitude=" + position.coords.latitude);
    });
};
$(function() {
    $("#id-lookaround-btn").click(function(evt) {
        if ($(this).attr("href").slice(-1) == "?") {
            swal({
                title: "Oops...",
                text: "You are not allowing getting location! Please allow us to get current location.",
                type: "warning",
                showCancelButton: false,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "OK!",
                closeOnConfirm: true,
                closeOnCancel: false
            }, function(isConfirm) {
                if (isConfirm) {
                    shareLocation();
                }
            });
            return false;
        }
    });
    shareLocation();
});
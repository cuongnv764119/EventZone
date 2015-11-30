$(function () {
    //Datetime Picker
    $("#dtpStartTime").datetimepicker({
        keyBinds: false
    });
    $("#dtpEndTime").datetimepicker({
        useCurrent: false,
        keyBinds: false
    });
    $('#dtpEndTime').keypress(function (event) {
        event.preventDefault();
    });
    $('#dtpStartTime').keypress(function (event) {
        event.preventDefault();
    });


    $("#dtpStartTime").on("dp.change", function (e) {
        $('#dtpEndTime').data("DateTimePicker").minDate(e.date);
    });
    $("#dtpEndTime").on("dp.change", function (e) {
        $('#dtpStartTime').data("DateTimePicker").maxDate(e.date);
    });
    function htmlEncode(value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    }
    $("#btnAddLocation").click(function() {
        var length = $("input[id^=Location-]").length;
        $("#LocationInput").append("<div id=\"wrapper\">" +
            "<div class=\"col-xs-8 col-sm-8 col-md-8 col-lg-8 col-xs-offset-2 col-sm-offset-2 col-md-offset-2 col-lg-offset-2 b\">" +
            "<input type=\"text\" name=\"Location[" + length + "].LocationName\" id=\"Location-" + length + "\" class=\"LocationInput form-control\" onfocus=\"searchLocation(this)\" />" +
            "<input type=\"hidden\" data-val=\"true\" data-val-number=\"The field Longitude must be a number.\" data-val-required=\"The Longitude field is required.\" name=\"Location[" + length + "].Longitude\" id=\"Longitude-" + length + "\"  class=\"LongitudeInput\" />" +
            "<input type=\"hidden\" data-val=\"true\" data-val-number=\"The field Lattitude must be a number.\" data-val-required=\"The Lattitude field is required.\" name=\"Location[" + length + "].Latitude\" id=\"Lattitude-" + length + "\" class=\"LangitudeInput\" />" +
            "</div>" +
            "<div class=\"col-xs-2 col-sm-2 col-md-2 col-lg-2 b\">" +
            "<button type=\"button\" id=\"btnRemove-"+length+"\"class=\"btn btn-primary btnRemoveLocation\">Remove</button>" +
            "</div>" +
            "</div>");
    });
        $("#LocationInput").on("click", ".btnRemoveLocation", function () { //user click on remove text
            $(this).parent("div").parent("div").hide();
            var id = this.id.substring(10, 11);
            var LocationId = "Location-" + id;
            document.getElementById(LocationId).value = "Remove Location";
        });
    $('#editor').on('summernote.change', function () {
        $("#event-description").val(htmlEncode($('#editor').code()).replace(/"/g, "'"));
    });
    //Binding Locations to One Location String
    $("#btnSubmit").click(function () {
        $(this).parents("form").submit();
    });

});
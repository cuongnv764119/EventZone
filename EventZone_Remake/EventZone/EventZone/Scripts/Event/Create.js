$(function() {
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


//Description
   /* function initToolbarBootstrapBindings() {
        var fonts = [
                "Serif", "Sans", "Arial", "Arial Black", "Courier",
                "Courier New", "Comic Sans MS", "Helvetica", "Impact", "Lucida Grande", "Lucida Sans", "Tahoma", "Times",
                "Times New Roman", "Verdana"
            ],
            fontTarget = $("[title=Font]").siblings(".dropdown-menu");
        $.each(fonts, function(idx, fontName) {
            fontTarget.append($("<li><a data-edit=\"fontName " + fontName + "\" style=\"font-family:'" + fontName + "'\">" + fontName + "</a></li>"));
        });
        $("a[title]").tooltip({ container: "body" });
        $(".dropdown-menu input").click(function() { return false; })
            .change(function() { $(this).parent(".dropdown-menu").siblings(".dropdown-toggle").dropdown("toggle"); })
            .keydown("esc", function() {
                this.value = "";
                $(this).change();
            });

        $("[data-role=magic-overlay]").each(function() {
            var overlay = $(this), target = $(overlay.data("target"));
            overlay.css("opacity", 0).css("position", "absolute").offset(target.offset()).width(target.outerWidth()).height(target.outerHeight());
        });
        if ("onwebkitspeechchange" in document.createElement("input")) {
            var editorOffset = $("#editor").offset();
            $("#voiceBtn").css("position", "absolute").offset({ top: editorOffset.top, left: editorOffset.left + $("#editor").innerWidth() - 35 });
        } else {
            $("#voiceBtn").hide();
        }
    };

    function showErrorAlert(reason, detail) {
        var msg = "";
        if (reason === "unsupported-file-type") {
            msg = "Unsupported format " + detail;
        } else {
            console.log("error uploading file", reason, detail);
        }
        $("<div class=\"alert\"> <button type=\"button\" class=\"close\" data-dismiss=\"alert\">&times;</button>" +
            "<strong>File upload error</strong> " + msg + " </div>").prependTo("#alerts");
    };

    initToolbarBootstrapBindings();
    $("#editor").wysiwyg({ fileUploadError: showErrorAlert });
    window.prettyPrint && prettyPrint();
    */
    /*$(".d_stream_cover").fadeOut("slow");
    $("#IsLive").change(function() {
        if (this.checked) {
            $(".d_stream_cover").fadeIn("slow");
        } else {
            $(".d_stream_cover").fadeOut("slow");
        }
    });*/
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
        //$("#i_location_1").empty();
        //$('[id^=Location-]').each(function (i, item) {
        //    var x = $(item).val();
        //    if (x.toString() != "Remove Location")
        //        $("#i_location_1").append(new Option($(item).val(), $(item).val(), true, true));
        //});
    });
    $("#LocationInput").on("click", ".btnRemoveLocation", function () { //user click on remove text
        $(this).parent("div").parent("div").hide();
        var id = this.id.substring(10, 11);
        var LocationId = "Location-" + id;
        document.getElementById(LocationId).value = "Remove Location";
        //$("#i_location_1").empty();
        //$('[id^=Location-]').each(function (i, item) {
        //    var x = $(item).val();
        //    if (x.toString() != "Remove Location")
        //        $("#i_location_1").append(new Option($(item).val(), $(item).val(), true, true));
        //});
    });
    $('#editor').on('summernote.change', function() {
        $("#event-description").val(htmlEncode($('#editor').code()).replace(/"/g, "'"));
    });
    //Binding Locations to One Location String
    $("#btnSubmit").click(function () {
        $(this).parents("form").submit();
    });
    function htmlEncode(value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    }
    //xu ly browse avatar
    function handleFileSelect(evt) {
        var files = evt.target.files; // FileList object

        // Loop through the FileList and render image files as thumbnails.
        for (var i = 0, f; f = files[i]; i++) {

            // Only process image files.
            if (!f.type.match('image.*')) {
                continue;
            }

            var reader = new FileReader();

            // Closure to capture the file information.
            reader.onload = (function (theFile) {
                return function (e) {
                    // Render thumbnail.
                    $("#imageView").empty();
                    var span = document.createElement('span');
                    span.innerHTML = ['<img class="d_thumb" src="', e.target.result,
                                      '" title="', escape(theFile.name), '"/>'].join('');
                    document.getElementById('imageView').insertBefore(span, null);
                };
            })(f);

            // Read in the image file as a data URL.
            reader.readAsDataURL(f);
        }
    }
    document.getElementById('image-upload-btn').addEventListener('change', handleFileSelect, false);
    //end xu ly browse avatar
});
$(document).ready(function () {
    $(".d_drop_cate").hide(0);
    $("#Keyword").toggle("slide", 0);

    $("#i_drdn_cate").mouseenter(function () {
        $(".d_drop_cate").slideToggle();
    });
    $(".d_btn_search").mouseenter(function () {
        $("#Keyword").toggle("slide");
    });
});

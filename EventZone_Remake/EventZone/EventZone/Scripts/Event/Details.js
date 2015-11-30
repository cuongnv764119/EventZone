$(document).ready(function () {

    $('#i_comment_btn').click(function () {
        $('.d_comment_report').animate({
            height: '500'
        }, {
            duration: 500,
            queue: false,   //queue: false giúp các Animation chạy đồng thời
            complete: function () { /* Animation complete */ }
        });

        $('.d_comment_report').animate({
            top: '-300'
        }, {
            duration: 500,
            queue: false,
            complete: function () { /* Animation complete */ }
        });

        $('.d_cmt_content_cover').addClass('d_cmt_extend');
        $('.d_cmt_content_cover').addClass('d_cmt_extend_2');
    });

    /*** OUTSOURCE: Google Map ***/
    /*** End of OUTSOURCE: Google Map ***/

    /*** OUTSOURCE: tùy chỉnh scroll bar ***/
    /*** End of OUTSOURCE: tùy chỉnh scroll bar ***/
});
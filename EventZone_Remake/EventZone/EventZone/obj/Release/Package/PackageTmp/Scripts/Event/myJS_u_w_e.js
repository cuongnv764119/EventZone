$(document).ready(function(){

/******************************************** SCRIPT xử lý ngay khi Load trang ***/
	$('.d_cmt_after').css("display", "none");	//Hide Comment
	$('.d_reopen_map').css("display", "none"); //Hide nút Re-open Map

/******************************************** Animation Mở Comment, Hide Map ***/
	$('.d_txt_write_cmt').click(function(){
		$('.d_right_bottom').animate({
		    height: '550px'
		}, {
		    duration: 600,
		    queue: false,   //queue: false giúp các Animation chạy đồng thời
		    complete: function() { /* Animation complete */ }
		});
		var d = $('#comment-content');
		d.scrollTop(d.prop("scrollHeight"));
		$('.d_right_top').slideToggle( "slow" ); //Hide Map đi
		$('.d_cmt_before').slideToggle( "slow" ); //Hide Sample comment
		$('.d_cmt_after').slideToggle( "slow" ); //Display Comment
		$('.d_reopen_map').slideToggle( "slow" ); //Display nút Re-open Map
	});

/******************************************** Animation Hide Comment, Mở Map ***/
	$('.d_reopen_map').click(function(){
		$('.d_right_bottom').animate({
		    height: '230px'
		}, {
		    duration: 600,
		    queue: false,   //queue: false giúp các Animation chạy đồng thời
		    complete: function() { /* Animation complete */ }
		});
		$('.d_right_top').slideToggle( "slow" ); //Mở lại Map
		$('.d_cmt_before').slideToggle( "slow" ); //Mở lại Sample comment
		$('.d_cmt_after').slideToggle( "slow" ); //Hide Comment
		$('.d_reopen_map').slideToggle( "slow" ); //Hide nút Re-open Map
	});
	


	
});
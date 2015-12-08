using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EventZone.Models
{
    public class CreateEventModel
    {
        [Required]
        [MaxLength(60, ErrorMessage = "The Title must less than 60 characters.")]
        [StringLength(100, ErrorMessage = "The {0} must be greater than {2} characters.", MinimumLength = 6)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        [Required]
        [Display(Name = "Location")]
        public List<Location> Location { get; set; }
        public string Quality { get; set; }
        public int PrivacyYoutube { get; set; }
        public DateTime StartTimeYoutube { get; set; }
        public DateTime EndTimeYoutube { get; set; }
        public string LocationLiveName { get; set; }
        [Required]
        [Display(Name = "Privacy")]
        public int Privacy { get; set; }
        [Required]
        [Display(Name = "Category")]
        public long CategoryID { get; set; }
        [MaxLength(2000, ErrorMessage = "The Description must less than 2000 characters.")]
        public string Description { get; set; }
    }

    public class EditViewModel
    {
        public long eventID { get; set; }
        [Required]
        [MaxLength(60, ErrorMessage = "The Title must less than 60 characters.")]
        [StringLength(100, ErrorMessage = "The {0} must be greater than {2} characters.", MinimumLength = 6)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        [Required]
        [Display(Name = "Location")]
        public List<Location> Location { get; set; }
        [Required]
        [Display(Name = "Privacy")]
        public int Privacy { get; set; }
        [MaxLength(2000, ErrorMessage = "The Description must less than 2000 characters.")]
        public string Description { get; set; }
    }
    public class LiveStreamingModel {
        public long eventID { get; set; }

        [Required(ErrorMessage = "Please enter your Title")]
        [MaxLength(50, ErrorMessage = "Title must less than 50 characters.")]
        public string Title { get; set; }
        public long EventPlaceID { get; set; }
        public string Quality { get; set; }
        [Required]
        public int PrivacyYoutube { get; set; }
        [Required]
        public DateTime StartTimeYoutube { get; set; }
        public DateTime EndTimeYoutube { get; set; }
    }

    public class ViewThumbEventModel
    {
        public Event evt { get; set; }
        public long numberLike { get; set; }
        public long numberDislike { get; set; }
        public long numberFollow { get; set; }
        public List<Location> location { get; set; }
    }

    public class ViewDetailEventModel
    {
        public User createdBy { get; set; }
        public long eventId { get; set; }
        public string eventAvatar { get; set; }
        public string eventName { get; set; }
        public long numberView { get; set; }
        public int NumberLike { get; set; }
        public int NumberDisLike { get; set; }
        public bool isVerified { get; set; }
        public LikeDislike FindLike { get; set; }
        public int NumberFowllower { get; set; }
        public string eventDescription { get; set; }
        public bool isFollowing { get; set; }
        public bool isOwningEvent { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Location> eventLocation { get; set; }
        public List<Image> eventImage { get; set; }
        public List<Video> eventVideo { get; set; }
        public List<Comment> eventComment { get; set; }
        public int Privacy { get; set; }
        public String Category { get; set; }
    }
    public class ThumbEventHomePage {
        public long EventID { get; set; }
        public string avatar { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsVeried { get; set; }
        public List<Location> listLocation { get; set; }
    }
    public class CommentViewModel
    {

        public long eventID { set; get; }
        public List<Comment> listComment { set; get; }
    }

}
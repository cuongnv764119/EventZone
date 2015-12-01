using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventZone.Models
{
    public class ChangeUserEmail
    {
        [Required(ErrorMessage = "Enter your email address.")]
        [EmailAddress(ErrorMessage = "The email format is incorrect.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public long UserID { get; set; }
    }

    public class StatisticViewModel
    {
        public Dictionary<string, int> EventCountStatistic { get; set; }
        public Dictionary<string, List<int>> EventCreatedEachMonth { get; set; }
        public Dictionary<Event, long> TopTenEvents { get; set; }
        public Dictionary<string, int> EventByStatus { get; set; }
        public Dictionary<Location, int> TopTenLocations { get; set; }
        public Dictionary<User, int> TopTenUser { get; set; }
        public Dictionary<string, int> GenderRate { get; set; }
        public Dictionary<string, int> GroupbyAge { get; set; }
        public Dictionary<string, int> ReportByType { get; set; }
        public Dictionary<string, int> ReportByStatus { get; set; }
        public Dictionary<string, int> AppealByStatus { get; set; }

    }
    public class UserCreatedByAdmin{
        [Required(ErrorMessage = "Please enter user name")]
        [RegularExpression("(^[a-zA-Z0-9,.'-]+$)", ErrorMessage = "User name is invalid. User name only accepts alphabet and numberic characters")]
        [MaxLength(25, ErrorMessage = "User name must more than 8 characters and less than 25 characters.")]
        [StringLength(100, ErrorMessage = "UserName must more than 8 character and less than 25 character", MinimumLength = 8)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, ErrorMessage = "The {0} must more than {2} characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "The email format is incorrect")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime UserDOB { get; set; }


        [MaxLength(25, ErrorMessage = "Fisrt Name must more than 2 characters and less than 25 characters.")]
        [RegularExpression("(^[a-zA-Z0-9 ,.'-]+$)", ErrorMessage = "Name is invalid. Name only accepts alphabet, numberic characters and white space")]
        [StringLength(100, ErrorMessage = "UserName must more than 2 character and less than 25 character", MinimumLength = 2)]
        public string UserFirstName { get; set; }
    }
}


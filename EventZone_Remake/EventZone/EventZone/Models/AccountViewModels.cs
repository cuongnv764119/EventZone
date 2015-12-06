using System;
using System.ComponentModel.DataAnnotations;

namespace EventZone.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class EditUserModel
    {
        public long UserID { get; set; }
        public string IDCard { get; set; }
        public string Phone { get; set; }
        public string Place { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime UserDOB { get; set; }

        [Required(ErrorMessage = "Please select gender")]
        public int Gender { get; set; }

        [Required(ErrorMessage = "Please enter your first name!")]
        [Display(Name = "First name")]
        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }
    }

    public class SignInViewModel
    {
        [Required(ErrorMessage = "Please enter your user name")]
        [RegularExpression("(^[a-zA-Z0-9,.'-]+$)", ErrorMessage = "Only Allowed Alphabet Character In Title")]
        [MaxLength(25, ErrorMessage = "User name must more than 8 characters and less than 25 characters.")]
        [StringLength(100, ErrorMessage = "UserName must more than 8 character and less than 25 character", MinimumLength = 8)]
        [Display(Name = "User name")]
      
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool Remember { get; set; }
    }

    public class GoogleAccountModel
    {
        [Required(ErrorMessage = "Please enter your user name")]
        [RegularExpression("(^[a-zA-Z0-9,.'-]+$)", ErrorMessage = "Only Allowed Alphabet Character In Title")]
        [MaxLength(25, ErrorMessage = "User name must more than 8 characters and less than 25 characters.")]
        [StringLength(100, ErrorMessage = "UserName must more than 8 character and less than 25 character", MinimumLength = 8)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(100, ErrorMessage = "The {0} must more than {2} characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Range(typeof(DateTime), "1/1/0001", "1/1/2016", ErrorMessage = "Value for {0} must be before {2}")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime UserDOB { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Place { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        [Required(ErrorMessage = "Please select gender")]
        public int Gender { get; set; }
    }

    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Please enter your user name")]
        [RegularExpression("(^[a-zA-Z0-9,.'-]+$)", ErrorMessage = "User name is invalid. User name only accepts alphabet and numberic characters")]
        [MaxLength(25, ErrorMessage = "User name must more than 8 characters and less than 25 characters.")]
        [StringLength(100, ErrorMessage = "UserName must more than 8 character and less than 25 character", MinimumLength = 8)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(100, ErrorMessage = "The {0} must more than {2} characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "The email format is incorrect")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Range(typeof(DateTime), "1/1/0001", "1/1/2016", ErrorMessage = "Value for {0} must be before 1/1/2016")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime UserDOB { get; set; }

        [Required(ErrorMessage = "Please select gender")]
        public int Gender { get; set; }
        [MaxLength(25, ErrorMessage = "User name must more than 2 characters and less than 25 characters.")]
        [RegularExpression("(^[a-zA-Z0-9 ,.'-]+$)", ErrorMessage = "Name is invalid. Name only accepts alphabet, numberic characters and white space")]
        [StringLength(100, ErrorMessage = "UserName must more than 2 character and less than 25 character", MinimumLength = 2)]
        [Required(ErrorMessage = "Please enter your first name")]
        [Display(Name = "First name")]
        public string UserLastName { get; set; }

        [MaxLength(25, ErrorMessage = "Last Name must more than 2 characters and less than 25 characters.")]
        [RegularExpression("(^[a-zA-Z0-9 ,.'-]+$)", ErrorMessage = "Name is invalid. Name only accepts alphabet, numberic characters and white space")]
        [StringLength(100, ErrorMessage = "UserName must more than 2 character and less than 25 character", MinimumLength = 2)]
        public string UserFirstName { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "The email format is incorrect.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
    public class ChangePasswordView {
        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(100, ErrorMessage = "The {0} must more than {2} characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(100, ErrorMessage = "The {0} must more than {2} characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
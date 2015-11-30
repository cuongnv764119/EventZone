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
}


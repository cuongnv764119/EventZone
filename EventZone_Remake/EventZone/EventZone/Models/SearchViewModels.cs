using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EventZone.Helpers;

namespace EventZone.Models
{
    public class BasicSearch
    {
        [Required]
        public string Keyword { get; set; }
    }

    public class AdvanceSearch
    {
        public string Keyword { get; set; }
        public long[] SelectedCategory { get; set; }
        public Location Location { get; set; }

        public bool IsLive { get; set; }

        public DateTime StartDateRange { get; set; }
        public DateTime FinishDateRange { get; set; }

        public IEnumerable<Category> Categories
        {
            get
            {
                var list = CommonDataHelpers.Instance.GetAllCategory();
                return list;
            }
        }
    }
}
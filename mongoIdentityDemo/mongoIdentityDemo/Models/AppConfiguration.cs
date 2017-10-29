using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mongoIdentityDemo.Models
{
    public class AppConfiguration
    {
        public AppConfiguration()
        {
            PopularPosts = new List<int>();
        }

        public string DisqusShortname { get; set; }
        public string SiteUrl { get; set; }
        public string GoogleAnalyticsId { get; set; }
        public List<int> PopularPosts { get; set; }
        public string Key { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{

    public class EngagementActivityServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<EngagementActivity> GetEngagementActivity()
        {
            return entities.EngagementActivities.ToList();
        }
    }

    public class EngagementActivity
    {
        [Key]
        public int EngagementID { get; set; }
        public string ActivityStr { get; set; }
        //[Required(ErrorMessage = "- Please Select Activity -")]
        public int? ActivityID { get; set; }
        //[Required(ErrorMessage = "- Please Input Engagement Name -")]
        public string Name { get; set; }
        public string Date_Start { get; set; }
        public string Date_End { get; set; }
        public string PICID { get; set; }
        public string SupervisorID { get; set; }
        public string TeamLeaderID { get; set; }
        public string MemberID { get; set; }
        public virtual Activity Activity { get; set;}
    }
}
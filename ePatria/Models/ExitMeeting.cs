using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ePatria.Models
{
    public class ExitMeeting
    {
        [Key]
        public int ExitMeetingID { get; set; }
        public int ActivityID { get; set; }
        public int EngagementID { get; set; }
        public DateTime? Date_Start { get; set; }
        public DateTime? Date_End { get; set; }
        public string Status { get; set; }
        public string StatusTanggapan { get; set; }
        public int OrganizationID { get; set; }
        public int LetterOfCommandID { get; set; }
        public string Reviewer1 { get; set; }
        public string Reviewer2 { get; set; }

        public virtual Activity Activity { get; set; }
        public virtual EngagementActivity EngagementActivity { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }
        public List<FieldWork> FieldWork { get; set; }

    }
}
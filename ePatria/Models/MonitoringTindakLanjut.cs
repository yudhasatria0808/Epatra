using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class MonitoringTindakLanjut
    {
        [Key]
        public int MonitoringID { get; set; }
        public int RCMDetailRiskControlIssueID { get; set; }
        public int? LetterOfCommandID { get; set; }
        public string Status { get; set; }
        public string Desc { get; set; }
        public virtual RCMDetailRiskControlIssue RCMDetailRiskControlIssue { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }
    }
}
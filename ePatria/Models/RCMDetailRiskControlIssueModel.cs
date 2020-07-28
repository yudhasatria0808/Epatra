using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class RCMDetailRiskControlIssue
    {
        [Key]
        public int RCMDetailRiskControlIssueID { get; set; }
        public int RCMDetailRiskControlID { get; set; }
        public string NoRef { get; set; }
        public string Title { get; set; }
        public string Fact { get; set; }
        public string Criteria { get; set; }
        public string Impact { get; set; }
        public string ImpactValue { get; set; }
        public string Cause { get; set; }
        public string Recommendation { get; set; }
        public string ManagementResponse { get; set; }
        public string FindingType { get; set; }
        public string Status { get; set; }
        public string Signification { get; set;}
        public string Due_Date { get; set; }
        public string PICID { get; set; }
        public string Status_Approval { get; set; }
        public string Close_Date { get; set; }
        public string CaseClose { get; set; }
        public virtual RCMDetailRiskControl RCMDetailRiskControl { get; set; }
        public string Status_App { get; set; }
    }
}
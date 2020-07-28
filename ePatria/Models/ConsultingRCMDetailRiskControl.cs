using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingRCMDetailRiskControl
    {
        [Key]
        public int ConsultingRCMDetailRiskControlID { get; set; }
        public int ConsultingRCMDetailRiskID { get; set; }
        public string ControlName { get; set; }
        public string Status { get; set; }
        public string RefNo { get; set; }
        public string Title { get; set; }
        [DataType(DataType.Text)]
        public string Criteria { get; set; }
        [DataType(DataType.Text)]
        public string Impact { get; set; }
        [DataType(DataType.Text)]
        public string ImpactValue { get; set; }
        [DataType(DataType.Text)]
        public string Cause { get; set; }
        [DataType(DataType.Text)]
        public string Recommendation { get; set; }
        public string ManagementResponse { get; set; }
        public int PICID { get; set; }
        public string FindingType { get; set; }
        public DateTime DueDate { get; set; }
        public string CaseCloseBool { get; set; }
        public DateTime CloseDate { get; set; }
        public string ReviewStatus { get; set; }
        public string keterangan { get; set; }
        public virtual ConsultingRCMDetailRisk ConsultingRCMDetailRisk { get; set; }
        //public virtual ICollection<ConsultingRCMDetailControlAuditStep> ConsultingRCMDetailControlAuditSteps { get; set; }
    }
}
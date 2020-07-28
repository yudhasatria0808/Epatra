using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingRCMDetailControlAuditStep
    {
        [Key]
        public int ConsultingRCMDetailControlAuditStepID { get; set; }
        public int ConsultingRCMDetailRiskControlID { get; set; }
        public string AuditStepName { get; set; }
        public string Status { get; set; }
        public string WorkDoneDescription { get; set; }
        public string WorkDoneResult { get; set; }
        public virtual ConsultingRCMDetailRiskControl ConsultingRCMDetailRiskControl { get; set; }
    }
}
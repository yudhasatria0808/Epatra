using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingRCMDetailRiskControlDetail
    {
        [Key]
        public int ConsultingRCMDetailRiskControlDetailID { get; set; }
        public int ConsultingRCMDetailRiskControlID { get; set; }
        public string WorkDoneDescription { get; set; }
        public string WorkDoneResult { get; set; }
        public virtual ConsultingRCMDetailRiskControl ConsultingRCMDetailRiskControl { get; set; }
    }
}
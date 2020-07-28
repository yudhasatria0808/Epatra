using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingRCMDetailRisk
    {
        [Key]
        public int ConsultingRCMDetailRiskID { get; set; }
        public int ConsultingRCMID { get; set; }
        public string RiskName { get; set; }
        public string Status { get; set; }
        public virtual ConsultingRiskControlMatrix ConsultingRiskControlMatrix { get; set; }
        //public virtual ICollection<ConsultingRCMDetailRiskControl> ConsultingRCMDetailRiskControls { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingRiskControlMatrix
    {
        [Key]
        public int ConsultingRCMID { get; set; }
        public int ConsultingBPMID { get; set; }
        public string SubBusinessProcess { get; set; }
        public string Objectives { get; set; }
        public virtual ConsultingBusinessProcess ConsultingBusinessProcess { get; set; }
        //public virtual ICollection<ConsultingRCMDetailRisk> ConsultingRCMDetailRisk { get; set; }
    }
}
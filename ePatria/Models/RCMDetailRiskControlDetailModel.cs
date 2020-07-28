using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class RCMDetailRiskControlDetail
    {
        [Key]
        public int RCMDetailRiskControlDetailID { get; set; }
        public int RCMDetailRiskControlID { get; set; }
        public string WorkDoneDescription { get; set; }
        public string WorkDoneResult { get; set; }
        public virtual RCMDetailRiskControl RCMDetailRiskControl { get; set; }
    }
}
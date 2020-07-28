using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class RCMRisk
    {
        [Key]
        public int RCMRiskID { get; set; }
        public int RiskControlMatrixID { get; set; }
        public string Nama { get; set; }
        public string Keterangan { get; set; }
        public virtual RiskControlMatrix RiskControlMatrix { get; set; }
    }
}
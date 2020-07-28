using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingFieldWork
    {
        [Key]
        public int ConsultingFieldWorkID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public string Desc { get; set; }
        public string Status { get; set; }
        public int? ConsultingRCMID { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
        public virtual ConsultingRiskControlMatrix ConsultingRiskControlMatrix { get; set; }
    }
}
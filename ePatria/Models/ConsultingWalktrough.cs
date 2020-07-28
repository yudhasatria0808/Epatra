using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingWalktrough
    {
        [Key]
        public int ConsultingWalktroughID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public int ActivityID { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
        //public virtual ICollection<ConsultingBusinessProcess> ConsultingBusinessProcess { get; set; }
    }
}
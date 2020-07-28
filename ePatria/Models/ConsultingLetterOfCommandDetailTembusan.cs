using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingLetterOfCommandDetailTembusan
    {
        [Key]
        public int ConsultingLetterOfCommandDetailTembusanID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public string Tembusan { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingLetterOfCommandDetailUntuk
    {
        [Key]
        public int ConsultingLetterOfCommandDetailUntukID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public string Untuk { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
    }
}
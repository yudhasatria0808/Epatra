using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingLetterOfCommandDetailDasar
    {
        [Key]
        public int ConsultingLetterOfCommandDetailDasarID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public string Dasar { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
    }
}
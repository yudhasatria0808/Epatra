using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingReporting
    {
        [Key]
        public int ConsultingReportingID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public int ActivityID { get; set; }
        public string NoLaporan { get; set; }
        public string Kepada { get; set; }
        public string Dari { get; set; }
        public string Lampiran { get; set; }
        public string Perihal { get; set; }
        [DataType(DataType.Text)]
        public string Hasil { get; set; }
        public int? ConsultingFieldWorkID { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
        public virtual ConsultingFieldWork ConsultingFieldWork { get; set; }
    }
}
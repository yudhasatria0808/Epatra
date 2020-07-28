using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingAuditQuery
    {
        [Key]
        public int ConsultingAuditQueryID { get; set; }
        public string No { get; set; }
        public int? ConsultingRCMDetailRiskControlDetailID { get; set; }
        public DateTime? Tanggal { get; set; }
        public string Kepada { get; set; }
        public int Dari { get; set; }
        public string Lampiran { get; set; }
        public string Perihal { get; set; }
        public string Pembuka { get; set; }
        public string IssueDesc { get; set; }
        public string Criteria { get; set; }
        public string Impact { get; set; }
        public string AuditeeClarification { get; set; }
        public string Penutup { get; set; }
        public string Status { get; set; }
        public string Folder { get; set; }
        public string Keterangan { get; set; }
        public virtual ConsultingRCMDetailRiskControlDetail ConsultingRCMDetailRiskControlDetail { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class AuditQuery
    {
        [Key]
        public int AuditQueryID { get; set; }
        public string No { get; set; }
        public int? RCMDetailRiskControlDetailID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
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
        public string KeterangnAuditQuery { get; set; }
        public virtual RCMDetailRiskControlDetail RCMDetailRiskControlDetail { get; set; }
    }
}
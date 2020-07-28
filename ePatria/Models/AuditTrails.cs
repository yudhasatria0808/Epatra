using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class AuditTrails
    {
        [Key]
        public int AuditTrailID { get; set; }
        public string AuditAction { get; set; }
        public string DataModel { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public int KeyFieldID { get; set; }
        public string ValueBefore { get; set; }
        public string ValueAfter { get; set; }
        public string Changes { get; set; }
        public string Desc { get; set; }
        public string Username { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class AuditTrailsTemp
    {
        public class AuditChange
        {
            public string DateTimeStamp { get; set; }
            public string AuditActionType { get; set; }
            public string AuditActionTypeName { get; set; }
            public string Username { get; set; }
            public int ValueId { get; set; }
            public List<AuditDelta> Changes { get; set; }
            public AuditChange()
            {
                Changes = new List<AuditDelta>();
            }
        }

        public class AuditDelta
        {
            public string FieldName { get; set; }
            public string ValueBefore { get; set; }
            public string ValueAfter { get; set; }
            public string Username { get; set; }
        }
    }
}
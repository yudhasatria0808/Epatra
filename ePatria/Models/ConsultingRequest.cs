using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingRequest
    {
        [Key]
        private readonly ePatriaDefault entities = new ePatriaDefault();
        public int ConsultingRequestID { get; set; }
        public string NoRequest { get; set; }
        public string NoSurat { get; set; }
        public int RequesterID { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime? Date_End { get; set; }
        public string EvaluationResult { get; set; }
        public string Status { get; set; }
        public string ActivityStr { get; set; }
        public int? ActivityID { get; set; }
        public virtual Activity Activity { get; set; }
        public string getRequesterName(int requesterId)
        {
            string requesterName = entities.Employees.Find(requesterId).Name;
            return requesterName;
        }
        //public virtual ICollection<ConsultingLetterOfCommand> ConsultingLetterOfCommand { get; set; }
    }
}
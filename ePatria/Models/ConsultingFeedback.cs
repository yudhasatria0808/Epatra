using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingFeedback
    {
        [Key]
        public int ConsultingFeedbackID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public int? ConsultingFieldWorkID { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
        public virtual ICollection<ConsultingFeedbackQuestionDetail> ConsultingFeedbackQuestionDetails { get; set; }
        public virtual ConsultingFieldWork ConsultingFieldWork { get; set; }
    }
}
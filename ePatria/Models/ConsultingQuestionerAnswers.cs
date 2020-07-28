using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingQuestionerAnswers
    {
        [Key]
        public int ConsultingQuestionerAnswerID { get; set; }
        public int QuestionerID { get; set; }
        public int EmployeeID { get; set; }
        public int ConsultingFeedBackQuestionDetailID { get; set; }
        public string Answer { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public virtual Questioner Questioner { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ConsultingFeedbackQuestionDetail ConsultingFeedbackQuestionDetail { get; set; }
    }

    public class ConsultingAnswerr
    {
        public int QuestionerID { get; set; }
        public string Answer { get; set; }
    }

    public class ConsultingQuestionerAnswersViewModel
    {
        public List<Questioner> ConsultingQuestioners { get; set; }
        public string Id { get; set; }

        public ConsultingQuestionerAnswersViewModel()
        {
            ConsultingQuestioners = new List<Questioner>();
        }
    }
}
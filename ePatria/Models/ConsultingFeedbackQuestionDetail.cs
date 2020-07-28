using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingFeedbackQuestionDetail
    {
        [Key]
        public int ConsultingFeedbackQuestionDetailID { get; set; }
        public int ConsultingFeedbackID { get; set; }
        public string QuestionID { get; set; }
        public int PicID { get; set; }
        public string FeedbackURL { get; set; }
        public string RandomURL { get; set; }
        public string Status { get; set; }

        public virtual Questioner Questioner { get; set; }
        public virtual ConsultingFeedback ConsultingFeedback { get; set; }
    }
}
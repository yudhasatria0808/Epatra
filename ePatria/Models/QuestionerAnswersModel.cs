using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class QuestionerAnswers
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();
        [Key]
        public int QuestionerAnswerID { get; set; }
        public int QuestionerID { get; set; }
        public int EmployeeID { get; set; }
        public int FeedBackQuestionDetailID { get; set; }
        public string Answer { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public virtual Questioner Questioner { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual FeedbackQuestionDetail FeedbackQuestionDetail { get; set; }
        public string getQuestionName(int questId)
        {
            string questName = entities.Questioners.Where(p => p.QuestionerID == questId).FirstOrDefault().Name;
            return questName;
        }
        public string getEmpName(int empId)
        {
            string empName = entities.Employees.Where(p => p.EmployeeID == empId).FirstOrDefault().Name;
            return empName;
        }

    }

    public class Answerr
    {
        public int QuestionerID { get; set; }
        public string Answer { get; set; }
    }

    public class QuestionerAnswersViewModel
    {
        public List<Questioner> Questioners { get; set; }
        public string Id { get; set; }

        public QuestionerAnswersViewModel()
        {
            Questioners = new List<Questioner>();
        }
    }
}
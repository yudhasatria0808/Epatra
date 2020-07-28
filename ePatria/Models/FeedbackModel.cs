using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ePatria.Models
{
    public class FeedbackServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<Feedback> GetFeedback()
        {
            return entities.Feedbacks.ToList();
        }

        public IEnumerable<Feedback> GetFeedbackPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Feedbacks
                .OrderBy(m => m.FeedbackID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllFeedback()
        {
            return entities.Feedbacks.Count();
        }


        public Feedback GetFeedbackDetail(int mCustID)
        {
            return entities.Feedbacks.Where(m => m.FeedbackID == mCustID).FirstOrDefault();
        }

        public bool AddFeedback(Feedback org)
        {
            try
            {
                entities.Feedbacks.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateFeedback(Feedback org)
        {
            try
            {
                Feedback data = entities.Feedbacks.Where(m => m.FeedbackID == org.FeedbackID).FirstOrDefault();

                data.FieldWorkID = org.FieldWorkID;
                data.Status = org.Status;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteFeedback(int mCustID)
        {
            try
            {
                Feedback data = entities.Feedbacks.Where(m => m.FeedbackID == mCustID).FirstOrDefault();
                entities.Feedbacks.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class Feedback
    {
        public int FeedbackID { get; set; }
        //public int AuditPlanningMemorandumID { get; set; }
        public int FieldWorkID { get; set; }
        public string Status { get; set; }

        //public virtual AuditPlanningMemorandum AuditPlanningMemorandum { get; set; }
        public virtual FieldWork FieldWork { get; set; }
        public virtual ICollection<FeedbackQuestionDetail> FeedbackQuestionDetails { get; set; }
    }

    public class PagedFeedbackModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<Feedback> Feedback { get; set; }
        public int PageSize { get; set; }


        public int FeedbackID { get; set; }
        //public int AuditPlanningMemorandumID { get; set; }
        public int FieldWorkID { get; set; }
        public string Status { get; set; }

        //public virtual AuditPlanningMemorandum AuditPlanningMemorandum { get; set; }
        public virtual FieldWork FieldWork { get; set; }
    }
}
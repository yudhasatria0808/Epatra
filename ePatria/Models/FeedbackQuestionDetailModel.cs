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
    public class FeedbackQuestionDetailServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<FeedbackQuestionDetail> GetFeedbackQuestionDetail()
        {
            return entities.FeedbackQuestionDetails.ToList();
        }

        public IEnumerable<FeedbackQuestionDetail> GetFeedbackQuestionDetailPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.FeedbackQuestionDetails
                .OrderBy(m => m.FeedbackQuestionDetailID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllFeedbackQuestionDetail()
        {
            return entities.FeedbackQuestionDetails.Count();
        }


        public FeedbackQuestionDetail GetFeedbackQuestionDetailDetail(int mCustID)
        {
            return entities.FeedbackQuestionDetails.Where(m => m.FeedbackQuestionDetailID == mCustID).FirstOrDefault();
        }

        public bool AddFeedbackQuestionDetail(FeedbackQuestionDetail org)
        {
            try
            {
                entities.FeedbackQuestionDetails.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateFeedbackQuestionDetail(FeedbackQuestionDetail org)
        {
            try
            {
                FeedbackQuestionDetail data = entities.FeedbackQuestionDetails.Where(m => m.FeedbackQuestionDetailID == org.FeedbackQuestionDetailID).FirstOrDefault();

                data.FeedbackID = org.FeedbackID;
                data.QuestionerID = org.QuestionerID;
                data.PICID = org.PICID;



                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteFeedbackQuestionDetail(int mCustID)
        {
            try
            {
                FeedbackQuestionDetail data = entities.FeedbackQuestionDetails.Where(m => m.FeedbackQuestionDetailID == mCustID).FirstOrDefault();
                entities.FeedbackQuestionDetails.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class FeedbackQuestionDetail
    {
        public int FeedbackQuestionDetailID { get; set; }
        public int FeedbackID { get; set; }
        public string QuestionerID { get; set; }
        public int PICID { get; set; }
        public int LetterofCommandID { get; set; }
        public string FeedbackURL { get; set; }
        public string RandomURL { get; set; }
        public string Status { get; set; }

        public virtual Feedback Feedback { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }
        public virtual Questioner Questioner { get; set; }


    }

    public class PagedFeedbackQuestionDetailModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<FeedbackQuestionDetail> FeedbackQuestionDetail { get; set; }
        public int PageSize { get; set; }


        public int FeedbackQuestionDetailID { get; set; }
        public int FeedbackID { get; set; }
        public string QuestionerID { get; set; }
        public int PICID { get; set; }

        public virtual Feedback Feedback { get; set; }
    }
}
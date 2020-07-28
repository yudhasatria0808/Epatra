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
    public class QuestionerServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }

        public IEnumerable<Questioner> GetQuestioner()
        {
            return entities.Questioners.ToList();
        }

        public IEnumerable<Questioner> GetQuestionerPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Questioners
                .OrderBy(m => m.Name)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllQuestioner()
        {
            return entities.Questioners.Count();
        }


        public Questioner GetQuestionerDetail(int mCustID)
        {
            return entities.Questioners.Where(m => m.QuestionerID == mCustID).FirstOrDefault();
        }

        public bool AddQuestioner(Questioner org)
        {
            try
            {
                entities.Questioners.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateQuestioner(Questioner org)
        {
            try
            {
                Questioner data = entities.Questioners.Where(m => m.QuestionerID == org.QuestionerID).FirstOrDefault();

                data.Type = org.Type;
                data.Name = org.Name;
                data.Value = org.Value;
                data.Status = org.Status;
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteQuestioner(int mCustID)
        {
            try
            {
                Questioner data = entities.Questioners.Where(m => m.QuestionerID == mCustID).FirstOrDefault();
                entities.Questioners.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class PagedQuestionerModel
    {
        public int TotalRows { get; set; }
        public int PageSize { get; set; }

        public int QuestionerID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
    }

    public class Questioner
    {

        public int QuestionerID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        [DataType(DataType.Text)]
        public string Value { get; set; }
        public string Status { get; set; }
    }
}
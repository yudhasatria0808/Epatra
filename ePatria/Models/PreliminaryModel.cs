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
    public class PreliminaryServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<Preliminary> GetPreliminary()
        {
            return entities.Preliminaries.ToList();
        }

        public IEnumerable<Preliminary> GetPreliminaryPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Preliminaries
                .OrderBy(m => m.Status)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllPreliminary()
        {
            return entities.Preliminaries.Count();
        }


        public Preliminary GetPreliminaryDetail(int mCustID)
        {
            return entities.Preliminaries.Where(m => m.PreliminaryID == mCustID).FirstOrDefault();
        }

        public bool AddPreliminary(Preliminary org)
        {
            try
            {
                entities.Preliminaries.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdatePreliminary(Preliminary org)
        {
            try
            {
                Preliminary data = entities.Preliminaries.Where(m => m.PreliminaryID == org.PreliminaryID).FirstOrDefault();

                data.ActivityID = org.ActivityID;
                data.NomorPreliminarySurvey = org.NomorPreliminarySurvey;
                data.Date_Start = org.Date_Start;
                data.Date_End = org.Date_End;
                data.Type = org.Type;
                data.EmployeeID = org.EmployeeID;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeletePreliminary(int mCustID)
        {
            try
            {
                Preliminary data = entities.Preliminaries.Where(m => m.PreliminaryID == mCustID).FirstOrDefault();
                entities.Preliminaries.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class Preliminary
    {
        public int PreliminaryID { get; set; }

        public string Type { get; set; }
        public string Status { get; set; }
        public string NomorPreliminarySurvey { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public int ActivityID { get; set; }
        public string EmployeeID { get; set; }
        
        public int? EngagementID { get; set; }

        //[Required(ErrorMessage = "PIC is required")]
        //[DisplayFormat(ConvertEmptyStringToNull = true)]
        //[StringLength(160, MinimumLength = 2)]
        //[Required]
        public string PICID { get; set; }

        public string SupervisorID { get; set; }
        public string TeamLeaderID { get; set; }
        public string MemberID { get; set; }

        public virtual EngagementActivity EngagementActivity { get; set; }
        public virtual Activity Activity { get; set; }
        //public virtual Employee Employee { get; set; }


    }

    public class PagedPreliminaryModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<Preliminary> Preliminary { get; set; }
        public int PageSize { get; set; }

        public string Type { get; set; }
        public string Status { get; set; }
        public string NomorPreliminarySurvey { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public int ActivityID { get; set; }
        public string EmployeeID { get; set; }

        public virtual Activity Activity { get; set; }
        public virtual Employee Employee { get; set; }

    }
}
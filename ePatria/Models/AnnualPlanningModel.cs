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
    public class AnnualPlanningServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<AnnualPlanning> GetAnnualPlanning()
        {
            return entities.AnnualPlannings.ToList();
        }

        public IEnumerable<AnnualPlanning> GetAnnualPlanningPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.AnnualPlannings
                .OrderBy(m => m.Status)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllAnnualPlanning()
        {
            return entities.AnnualPlannings.Count();
        }


        public AnnualPlanning GetAnnualPlanningDetail(int mCustID)
        {
            return entities.AnnualPlannings.Where(m => m.AnnualPlanningID == mCustID).FirstOrDefault();
        }

        public bool AddAnnualPlanning(AnnualPlanning org)
        {
            try
            {
                entities.AnnualPlannings.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateAnnualPlanning(AnnualPlanning org)
        {
            try
            {
                AnnualPlanning data = entities.AnnualPlannings.Where(m => m.AnnualPlanningID == org.AnnualPlanningID).FirstOrDefault();
                data.Approval_Status = org.Approval_Status;
                data.Date_Start = org.Date_Start;
                data.Date_End = org.Date_End;
                data.ActivityID = org.ActivityID;
                data.Status = org.Status;
                data.Tahun = org.Tahun;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteAnnualPlanning(int mCustID)
        {
            try
            {
                AnnualPlanning data = entities.AnnualPlannings.Where(m => m.AnnualPlanningID == mCustID).FirstOrDefault();
                entities.AnnualPlannings.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class AnnualPlanning
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();
        public int AnnualPlanningID { get; set; }
        public DateTime? Date_Start { get; set; }       
        public DateTime? Date_End { get; set; }
        public string Status { get; set; }
        public string Approval_Status { get; set; }
        [Required(ErrorMessage = "- Please Input Tahun -")]
        public string Tahun { get; set; }
        [Required(ErrorMessage = "- Please Select Activity -")]
        public int ActivityID { get; set; }
        public string Type { get; set; }

        public virtual Activity Activity { get; set; }
        public virtual Employee Employee { get; set; }
        public List<EngagementActivity> Enga { get; set; }
        public List<EngagementActivity> getEngagementActivityByActivityID(int ActivityID)
        {
            List<EngagementActivity> engList = entities.EngagementActivities.Where(p => p.ActivityID == ActivityID).ToList();
            return engList;
        }

    }

    public class PagedAnnualPlanningModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<AnnualPlanning> AnnualPlanning { get; set; }
        public int PageSize { get; set; }


        public int AnnualPlanningID { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public string Status { get; set; }
        public string Approval_Status { get; set; }
        public string Tahun { get; set; }
        public int ActivityID { get; set; }
        public int PICID { get; set; }
        public int SupervisorID { get; set; }
        public int TeamLeaderID { get; set; }
        public int MemberID { get; set; }
        public virtual Activity Activity { get; set; }

    }
}
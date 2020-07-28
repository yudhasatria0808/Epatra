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
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Models
{
    public class WalktroughServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<Walktrough> GetWalktrough()
        {
            return entities.Walktroughs.ToList();
        }

        public IEnumerable<Walktrough> GetWalktroughPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Walktroughs
                .OrderBy(m => m.Status)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllWalktrough()
        {
            return entities.Walktroughs.Count();
        }


        public Walktrough GetWalktroughDetail(int mCustID)
        {
            return entities.Walktroughs.Where(m => m.WalktroughID == mCustID).FirstOrDefault();
        }

        public bool AddWalktrough(Walktrough org)
        {
            try
            {
                entities.Walktroughs.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateWalktrough(Walktrough org)
        {
            try
            {
                Walktrough data = entities.Walktroughs.Where(m => m.WalktroughID == org.WalktroughID).FirstOrDefault();

                data.ActivityID = org.ActivityID;
                data.PreliminaryID = org.PreliminaryID;
                data.Date_Start = org.Date_Start;
                data.Date_End = org.Date_End;
                data.Remarks = org.Remarks;
                data.Status = org.Status;
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteWalktrough(int mCustID)
        {
            try
            {
                Walktrough data = entities.Walktroughs.Where(m => m.WalktroughID == mCustID).FirstOrDefault();
                entities.Walktroughs.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class Walktrough
    {
        public int WalktroughID { get; set; }
        public int ActivityID { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public int? PreliminaryID { get; set; }

        public virtual Activity Activity { get; set; }
        public virtual Preliminary Preliminary { get; set; }
        public List<BusinessProces> BusinessProces { get; set; }
        public List<BPM> BPM { get; set; }
        public List<RiskControlMatrix> RCM { get; set; }

        public Employee getEmployeeByUserName(string username)
        {
            ePatriaDefault db = new ePatriaDefault();
            ApplicationUser user = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(username).Result;
            string fullname = user.FirstName + " " + user.LastName;
            string email = user.Email;
            Employee emp = db.Employees.Where(p => p.Email.Equals(email) && p.Name.Equals(fullname)).FirstOrDefault();
            return emp;
        }
    }

    public class PagedWalktroughModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<Walktrough> Walktrough { get; set; }
        public int PageSize { get; set; }

        public int WalktroughID { get; set; }
        public int PreliminaryID { get; set; }
        public int ActivityID { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public virtual Activity Activity { get; set; }
        public virtual Preliminary Preliminary { get; set; }

    }
}
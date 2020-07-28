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
    public class FieldWorkServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<FieldWork> GetFieldWork()
        {
            return entities.FieldWorks.ToList();
        }

        public IEnumerable<FieldWork> GetFieldWorkPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.FieldWorks
                .OrderBy(m => m.FieldWorkID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllFieldWork()
        {
            return entities.FieldWorks.Count();
        }


        public FieldWork GetFieldWorkDetail(int mCustID)
        {
            return entities.FieldWorks.Where(m => m.FieldWorkID == mCustID).FirstOrDefault();
        }

        public bool AddFieldWork(FieldWork org)
        {
            try
            {
                entities.FieldWorks.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateFieldWork(FieldWork org)
        {
            try
            {
                FieldWork data = entities.FieldWorks.Where(m => m.FieldWorkID == org.FieldWorkID).FirstOrDefault();

                data.RiskControlMatrixID = org.RiskControlMatrixID;
                data.LetterOfCommandID = org.LetterOfCommandID;
                data.Status = org.Status;
               


                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteFieldWork(int mCustID)
        {
            try
            {
                FieldWork data = entities.FieldWorks.Where(m => m.FieldWorkID == mCustID).FirstOrDefault();
                entities.FieldWorks.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class FieldWork
    {
        public int FieldWorkID { get; set; }
        public int LetterOfCommandID { get; set; }
        public int RiskControlMatrixID { get; set; }
        public string Status { get; set; }

        public virtual LetterOfCommand LetterOfCommand { get; set; }
        public virtual RiskControlMatrix RiskControlMatrix { get; set; }

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

    public class PagedFieldWorkModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<FieldWork> FieldWork { get; set; }
        public int PageSize { get; set; }

        public int FieldWorkID { get; set; }
        public int LetterOfCommandID { get; set; }
        public int RiskControlMatrixID { get; set; }
        public string Status { get; set; }

        public virtual LetterOfCommand LetterOfCommand { get; set; }
        public virtual RiskControlMatrix RiskControlMatrix { get; set; }
    }
}
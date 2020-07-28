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
    public class LetterOfCommandServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<LetterOfCommand> GetLetterOfCommand()
        {
            return entities.LetterOfCommands.ToList();
        }

        public IEnumerable<LetterOfCommand> GetLetterOfCommandPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.LetterOfCommands
                .OrderBy(m => m.Date_Start)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllLetterOfCommand()
        {
            return entities.LetterOfCommands.Count();
        }


        public LetterOfCommand GetLetterOfCommandDetail(int mCustID)
        {
            return entities.LetterOfCommands.Where(m => m.LetterOfCommandID == mCustID).FirstOrDefault();
        }

        public bool AddLetterOfCommand(LetterOfCommand org)
        {
            try
            {
                entities.LetterOfCommands.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateLetterOfCommand(LetterOfCommand org)
        {
            try
            {
                LetterOfCommand data = entities.LetterOfCommands.Where(m => m.LetterOfCommandID == org.LetterOfCommandID).FirstOrDefault();

                data.NomorSP = org.NomorSP;
                data.ActivityID = org.ActivityID;
                data.Status = org.Status;
                data.Remarks = org.Remarks;
                data.AssuranceType = org.AssuranceType;
                data.Date_Start = org.Date_Start;
                data.Date_End = org.Date_End;
                data.Menimbang = org.Menimbang;
                data.Penutup = org.Penutup;
                data.PreliminaryID = org.PreliminaryID;
                data.PICID = org.PICID;
                data.ActivityID = org.ActivityID;
                data.PICID = org.PICID;
                data.SupervisorID = org.SupervisorID;
                data.TeamLeaderID = org.TeamLeaderID;
                data.MemberID = org.MemberID;


                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteLetterOfCommand(int mCustID)
        {
            try
            {
                LetterOfCommand data = entities.LetterOfCommands.Where(m => m.LetterOfCommandID == mCustID).FirstOrDefault();
                entities.LetterOfCommands.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class LetterOfCommand
    {
        public int LetterOfCommandID { get; set; }
        public string NomorSP { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string AssuranceType { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        [DataType(DataType.Text)]
        public string Menimbang { get; set; }
        [DataType(DataType.Text)]
        public string Penutup { get; set; }
        public int? PreliminaryID { get; set; }
        public int? AuditPlanningMemorandumID { get; set; }
        public int ActivityID { get; set; }
        public string PICID { get; set; }
        public string SupervisorID { get; set; }
        public string TeamLeaderID { get; set; }
        public string MemberID { get; set; }

        public virtual Activity Activity { get; set; }
        public virtual Preliminary Preliminary { get; set; }
        public virtual AuditPlanningMemorandum AuditPlanningMemorandum { get; set; }
        private readonly ePatriaDefault entities = new ePatriaDefault();
        public Employee getEmployeeByUserName(string username)
        {
            ePatriaDefault db = new ePatriaDefault();
            ApplicationUser user = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(username).Result;
            string fullname = user.FirstName + " " + user.LastName;
            string email = user.Email;
            Employee emp = db.Employees.Where(p => p.Email.Equals(email) && p.Name.Equals(fullname)).FirstOrDefault();
            return emp;
        }

        public string getNoPekEmpByName(string empName)
        {
            string noPek = entities.Employees.Where(p => p.Name == empName).FirstOrDefault().NoPEK;
            return noPek;
        }

        public string getRoleNameByEmpName(string empName)
        {
            string username = entities.Employees.Where(p => p.Name == empName).FirstOrDefault().UserName;
            string roleName = string.Empty;
            if (!String.IsNullOrEmpty(username))
            {
                string roleId = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(username).Result.Roles.FirstOrDefault().RoleId;
                roleName = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationRoleManager>().FindByIdAsync(roleId).Result.Name;
            }
            return roleName;
        }
    }


    public class PagedLetterOfCommandModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<LetterOfCommand> LetterOfCommand { get; set; }
        public int PageSize { get; set; }


        public int LetterOfCommandID { get; set; }
        public string NomorSP { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string AssuranceType { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public string Menimbang { get; set; }
        public string Penutup { get; set; }
        public int PreliminaryID { get; set; }
        public int ActivityID { get; set; }
        public int PICID { get; set; }
        public int SupervisorID { get; set; }
        public int TeamLeaderID { get; set; }
        public int MemberID { get; set; }
        public int ReviewMasterID { get; set; }
        public virtual Activity Activity { get; set; }
        public virtual Preliminary Preliminary { get; set; }

    }
}
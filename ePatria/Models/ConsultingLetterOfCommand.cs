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
    public class ConsultingLetterOfCommand
    {
        [Key]
        public int ConsultingSuratPerintahID { get; set; }
        public string NomorSP { get; set; }
        public int ConsultingRequestID { get; set; }
        public int? EngagementID { get; set; }
        public string EngagementName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PicID { get; set; }
        public string SupervisorID { get; set; }
        public string TeamLeaderID { get; set; }
        public string MemberID { get; set; }
        [DataType(DataType.Text)]
        public string Menimbang { get; set; }
        [DataType(DataType.Text)]
        public string Penutup { get; set; }
        public string Remarks { get; set; }
        public virtual ConsultingRequest ConsultingRequest { get; set; }
        public virtual EngagementActivity EngagementActivity { get; set; }
        //public List<ConsultingLetterOfCommandMember> ConsultingLetterOfCommandMember { get; set; }
        //public List<ConsultingLetterOfCommandDetailDasar> ConsultingLetterOfCommandDetailDasar { get; set; }
        //public List<ConsultingLetterOfCommandDetailUntuk> ConsultingLetterOfCommandDetailUntuk { get; set; }

        //public virtual ICollection<ConsultingLetterOfCommandMember> ConsultingLetterOfCommandMembers { get; set; }
        //public virtual ICollection<ConsultingLetterOfCommandDetailDasar> ConsultingLetterOfCommandDetailDasars { get; set; }
        //public virtual ICollection<ConsultingLetterOfCommandDetailUntuk> ConsultingLetterOfCommandDetailUntuks { get; set; }
        //public virtual ICollection<ConsultingLetterOfCommandDetailTembusan> ConsultingLetterOfCommandDetailTembusans { get; set; }
        //public virtual ICollection<ConsultingWalktrough> ConsultingWalktroughs { get; set; }
        //public virtual ICollection<ConsultingReporting> ConsultingReportings { get; set; }
        //public virtual ICollection<ConsultingFeedback> ConsultingFeedbacks { get; set; }
        //public virtual ICollection<ConsultingFieldWork> ConsultingFieldWorks { get; set; }
        private readonly ePatriaDefault entities = new ePatriaDefault();
        public string getNoPekEmpByName(string empName)
        {
            string noPek = entities.Employees.Where(p => p.Name == empName).FirstOrDefault().NoPEK;
            return noPek;
        }
        public Employee getEmployeeByUserName(string username)
        {
            ePatriaDefault db = new ePatriaDefault();
            ApplicationUser user = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(username).Result;
            string fullname = user.FirstName + " " + user.LastName;
            string email = user.Email;
            Employee emp = db.Employees.Where(p => p.Email.Equals(email) && p.Name.Equals(fullname)).FirstOrDefault();
            return emp;
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
}
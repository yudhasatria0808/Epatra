using ePatria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace ePatria.Controllers
{
    [Authorize]
    public class HelperController
    {
        public static string GetStatusSendback(ePatriaDefault db ,string menuName, string OldStatus)
        {
            int idmenu = db.Permissions.Where(d => d.PermissionName == menuName).Select(d => d.permissionID).FirstOrDefault();
            var iscreateId = db.PermissionRoles.Where(d => d.permissionID == idmenu && d.IsCreate == true).Select(d => d.roleID).FirstOrDefault();
            var issubmit1 = db.PermissionRoles.Where(d => d.permissionID == idmenu && d.IsSubmit1 == true).Select(d => d.roleID).FirstOrDefault();
            var issubmit2 = db.PermissionRoles.Where(d => d.permissionID == idmenu && d.IsSubmit2 == true).Select(d => d.roleID).FirstOrDefault();
            var isapprove = db.PermissionRoles.Where(d => d.permissionID == idmenu && d.IsApprove == true).Select(d => d.roleID).FirstOrDefault();
            if (OldStatus != null && OldStatus != "")
            {
                if (OldStatus.Contains("Approve")) // approve
                {
                    //jika statusnya berkaitan approve
                    if (issubmit2 != null && issubmit2 != "")
                    {
                        return "Pending for Review by " + HelperController.getRoleName(issubmit2);
                    }
                    else if (issubmit1 != null && issubmit1 != "")
                    {
                        return "Pending for Review by " + HelperController.getRoleName(issubmit1);
                    }
                    else
                    {
                        //annualPlanning.Approval_Status = "Pending for Review by";
                        return "Draft";
                    }
                }
                else // submit 2
                {
                    if (issubmit1 != null && issubmit1 != "" && issubmit1 != iscreateId)
                    {
                        return "Pending for Review by " + HelperController.getRoleName(issubmit1);
                    }
                    else if (issubmit2 != null && issubmit2 != "" && issubmit2 != iscreateId)
                    {
                        return "Pending for Review by " + HelperController.getRoleName(issubmit2);
                    }
                    else if (isapprove != null && isapprove != "" && isapprove != iscreateId)
                    {
                        return "Pending for Approve by " + HelperController.getRoleName(isapprove);
                    }
                    else
                    {
                        return "Draft";
                    }
                }
            }
            return "Draft";
        }

        public static List<PermissionRoles> getPermission(string userID, string pername)
        {
            ePatriaDefault db = new ePatriaDefault();
            List<string> roleId = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userID).Roles.Select(p => p.RoleId).ToList();
            List<PermissionRoles> permission = db.PermissionRoles.Where(p => (roleId.Contains(p.roleID)) && p.Permissions.PermissionName.Equals(pername)).ToList();
            return permission;
        }

        public static string getRoleName(string roleID)
        {
            ePatriaDefault db = new ePatriaDefault();
            string roleName = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Id.Equals(roleID)).Select(p => p.Name).FirstOrDefault();
            return roleName;
        }

        public static bool isSuperAdmin(string userID)
        {
            bool result = false;
            ePatriaDefault db = new ePatriaDefault();
            string roleId = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userID).Roles.Select(p => p.RoleId).FirstOrDefault();
            string roleName = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Id.Equals(roleId)).Select(p => p.Name).FirstOrDefault();
            if (roleName == "Superadmin")
                result = true;
            return result;
        }

        public static string getPermissionRoleByPerm(string pername, string userId)
        {
            ePatriaDefault db = new ePatriaDefault();
            //string roleId = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userID).Roles.Select(p => p.RoleId).FirstOrDefault();
            List<PermissionRoles> permission = db.PermissionRoles.Where(p => p.Permissions.PermissionName.Equals(pername)).ToList();
            var isCreate = new List<PermissionRoles>();
            var isRead = new List<PermissionRoles>();
            var isUpdate = new List<PermissionRoles>();
            var isDelete = new List<PermissionRoles>();
            var isSubmit1 = new List<PermissionRoles>();
            var isSubmit2 = new List<PermissionRoles>();
            var isApprove = new List<PermissionRoles>();
            var allAccess = String.Empty;
            isCreate = permission.Where(p => p.IsCreate == true).ToList();
            isRead = permission.Where(p => p.IsRead == true).ToList();
            isUpdate = permission.Where(p => p.IsUpdate == true).ToList();
            isDelete = permission.Where(p => p.IsDelete == true).ToList();
            isSubmit1 = permission.Where(p => p.IsSubmit1 == true).ToList();
            isSubmit2 = permission.Where(p => p.IsSubmit2 == true).ToList();
            isApprove = permission.Where(p => p.IsApprove == true).ToList();
            if (isCreate.Count() > 0)
                allAccess = "IsCreate;";
            if (isUpdate.Count() > 0)
                allAccess += "IsUpdate;";
            if (isRead.Count() > 0)
                allAccess += "IsRead;";
            if (isDelete.Count() > 0)
                allAccess += "IsDelete;";
            if (isSubmit1.Count() > 0)
                allAccess += "IsSubmit1:" + getRoleName(isSubmit1.Select(p => p.roleID).FirstOrDefault()) + ";";
            else
                allAccess += ":;";
            if (isSubmit2.Count() > 0)
                allAccess += "IsSubmit2:" + getRoleName(isSubmit2.Select(p => p.roleID).FirstOrDefault()) + ";";
            else
                allAccess += ":;";
            if (isApprove.Count() > 0)
                allAccess += "IsApprove:" + getRoleName(isApprove.Select(p => p.roleID).FirstOrDefault()) + ";";
            else
                allAccess += ":;";
            //bool isSuperAdmin = HelperController.isSuperAdmin(userId);
            //if (isSuperAdmin)
            //    allAccess = "IsCreate;IsUpdate;IsRead;IsDelete;IsSubmit1:" + getRoleName(isSubmit1.Select(p => p.roleID).FirstOrDefault()) + ";" +
            //        "IsSubmit2:" + getRoleName(isSubmit2.Select(p => p.roleID).FirstOrDefault()) + ";" + "IsApprove:" +
            //        getRoleName(isApprove.Select(p => p.roleID).FirstOrDefault()) + ";";
            return allAccess;
        }

        public static string getUserNameByEmpName(string empName)
        {
            ePatriaDefault db = new ePatriaDefault();
            Employee emp = db.Employees.Where(p => p.Name.Equals(empName)).FirstOrDefault();
            string uName = String.Empty;
            if (emp != null)
                uName = emp.UserName;
            
            return uName;
        }

        public static string getNameByUserName(string uName)
        {
            ePatriaDefault db = new ePatriaDefault();
            string firstName = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByName(uName).FirstName;
            string lastName = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByName(uName).LastName;
            string fullName = firstName + " " + lastName;

            return fullName;
        }

        public string getNewNumber(string type)
        {
            ePatriaDefault db = new ePatriaDefault();
            string result = String.Empty;
            int length = 4;
            int no = 1;
            string newNo = string.Empty;
            int year = DateTime.Now.Year;
            bool noExist = false;
            string formatNo = string.Empty;
            if (type == "SP") 
            {
                LetterOfCommand sp = db.LetterOfCommands.OrderByDescending(p => p.LetterOfCommandID).FirstOrDefault();
                if (sp != null)
                {
                    noExist = db.LetterOfCommands.Any(p => p.NomorSP.Contains("/PPN030/Prin/"));
                    if (noExist)
                        no = Convert.ToInt32(sp.NomorSP.Split('/')[0]) + 1;
                }
                newNo = no.ToString().PadLeft(length, '0');
                result = newNo + "/PPN030/Prin/" + year;
            }
            else if (type == "Reporting") {
                Reporting rep = db.Reportings.OrderByDescending(p => p.ReportingID).FirstOrDefault();
                if (rep != null)
                {
                    noExist = db.Reportings.Any(p => p.NomorLaporan.Contains("/PPN030/Lap/"));
                    if (noExist)
                        no = Convert.ToInt32(rep.NomorLaporan.Split('/')[0]) + 1;
                }
                newNo = no.ToString().PadLeft(length, '0');
                result = newNo + "/PPN030/Lap/" + year;
            } 
            else if (type == "CSP") {
                ConsultingLetterOfCommand csp = db.ConsultingLetterOfCommands.OrderByDescending(p => p.ConsultingSuratPerintahID).FirstOrDefault();
                if (csp != null)
                {
                    noExist = db.ConsultingLetterOfCommands.Any(p => p.NomorSP.Contains("/PPN030/CPrin/"));
                    if (noExist)
                        no = Convert.ToInt32(csp.NomorSP.Split('/')[0]) + 1;
                }
                newNo = no.ToString().PadLeft(length, '0');
                result = newNo + "/PPN030/CPrin/" + year;
            }
            else if (type == "CReporting")
            {
                ConsultingReporting crep = db.ConsultingReportings.OrderByDescending(p => p.ConsultingReportingID).FirstOrDefault();
                if (crep != null)
                {
                    noExist = db.ConsultingReportings.Any(p => p.NoLaporan.Contains("/PPN030/CLap/"));
                    if (noExist)
                        no = Convert.ToInt32(crep.NoLaporan.Split('/')[0]) + 1;
                }
                newNo = no.ToString().PadLeft(length, '0');
                result = newNo + "/PPN030/CLap/" + year;
            }
            else if (type == "CRequest")
            {
                ConsultingRequest creq = db.ConsultingRequests.OrderByDescending(p => p.ConsultingRequestID).FirstOrDefault();
                if (creq != null)
                {
                    noExist = db.ConsultingRequests.Any(p => p.NoRequest.Contains("/PPN030/"));
                    if (noExist)
                        no = Convert.ToInt32(creq.NoRequest.Split('/')[0]) + 1;
                }
                newNo = no.ToString().PadLeft(length, '0');
                result = newNo + "/PPN030/" + year;
            }
            return result;
        }

        public static List<string> getUsersByRole(string roleName)
        {
            ePatriaDefault db = new ePatriaDefault();
            List<Employee> employees = new List<Employee>();
            List<ApplicationUser> users = new List<ApplicationUser>();
            List<ApplicationUser> allUsers = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.ToList();
            string roleId = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationRoleManager>().FindByName(roleName).Id;
            foreach (var user in allUsers)
            {
                if (user.Roles.Any(p => p.RoleId == roleId))
                {
                    Employee emp = db.Employees.Where(p => p.UserName == user.UserName).FirstOrDefault();
                    if (emp != null)
                        employees.Add(emp);
                }
            }
            List<string> empNames = employees.Select(p => p.Name).ToList();
            return empNames;
        }

        public static string convertDay(string day) {
            string newDay = string.Empty;
            if (day == "Sunday")
                newDay = "Minggu";
            else if (day == "Monday")
                newDay = "Senin";
            else if (day == "Tuesday")
                newDay = "Selasa";
            else if (day == "Wednesday")
                newDay = "Rabu";
            else if (day == "Thursday")
                newDay = "Kamis";
            else if (day == "Friday")
                newDay = "Jumat";
            else if (day == "Saturday")
                newDay = "Sabtu";
            return newDay;
        }

        public static bool IsMemberSp(int idSP ,string uName)
        { 
            ePatriaDefault db = new ePatriaDefault();

            LetterOfCommand dbsp = db.LetterOfCommands.Where(d => d.LetterOfCommandID == idSP).FirstOrDefault();

            bool result = dbsp.MemberID.Split(';').Contains(uName);

            db.Dispose();

            return result;
        }

        public static bool IsLeaderSp(int idSP, string uName)
        {
            ePatriaDefault db = new ePatriaDefault();

            bool result = db.LetterOfCommands.Any(d => d.LetterOfCommandID == idSP && d.TeamLeaderID == uName);

            db.Dispose();

            return result;
        }

        public static bool IsSupervisorSp(int idSP, string uName)
        {
            ePatriaDefault db = new ePatriaDefault();

            bool result = db.LetterOfCommands.Any(d => d.LetterOfCommandID == idSP && d.SupervisorID == uName);

            db.Dispose();

            return result;
        }
    }
}
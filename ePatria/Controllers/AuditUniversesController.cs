using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria;
using ePatria.Models;
using System.IO;
using Microsoft.AspNet.Identity.Owin;


namespace ePatria.Controllers
{
    [Authorize()]
    public class AuditUniversesController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        ActivityServices mobjModel = new ActivityServices();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        // GET: AuditUniverse Popup
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            string messageerror = TempData["messageerror"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            else if (!string.IsNullOrEmpty(messageerror))
            {
                ViewBag.messageerror = messageerror;
            }
            List<Activity> auditUniverses = new List<Activity>();
            auditUniverses = db.Activities.ToList();
            ViewBag.AuditUniverseIndex = auditUniverses;


            List<Activity> all = new List<Activity>();
            using (ePatriaDefault mydb = new ePatriaDefault())
            {
                all = mydb.Activities.OrderBy(a => a.ActivityParentID).ToList();
            }
            return View(all);
        }

        // Create Popup AuditUniverse
        public ActionResult CreateAuditUniverse(string status,int? activityparentid)
        {
            ViewBag.ActivityParentID = activityparentid;
            ViewBag.Status = status;
            ViewBag.DepartementID = new SelectList(db.Organizations, "OrganizationID", "Name");

                return View();
        }

        public ActionResult CreateSubAuditUniverse(string status, int activityparentid)
        {
            ViewBag.DepartementID = new SelectList(db.Organizations, "OrganizationID", "Name");

            var data = mobjModel.GetActivityDetail(activityparentid);
            Activity audituniverse = db.Activities.Find(activityparentid);
            ViewBag.ActivityParentID = activityparentid;
           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAuditUniverse(Activity org, [Bind(Include = "ActivityID,ActivityParentID,Name,Description,Tahun,DepartementID,Status,Status_Active")] Activity activity, string submit)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    activity.Status = "Draft";
                else if (submit == "Send Back")
                    activity.Status = HelperController.GetStatusSendback(db, "Audit Universe", activity.Status);
                else if (submit == "Approve")
                    activity.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    activity.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    activity.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {
                            Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                            string emailContent = "Dear {0}, <BR/><BR/>Audit Universe : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Audit Universe\">link</a> to show the Audit Universe.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                            string url = baseUrl + "/AuditUniverses/Details?ActivityID=" + org.ActivityID;
                            emailTransact.SentEmailApproval(emp.Email, emp.Name, org.Name, emailContent, url);
                        }
                    }
                }
                db.Activities.Add(activity);
                db.SaveChanges();
                Activity ac = new Activity();
                auditTransact.CreateAuditTrail("Create", activity.ActivityID, "AuditUniverse", ac, activity, username);
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "au";
                rrm.Description = page + activity.ActivityID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();
                TempData["message"] = "Audit Universe successfully created!";
                return RedirectToAction("Index");
            }

            return View(activity);
        }
       
        public ActionResult EditAuditUniverse(int activityid)
        {   
            //new SelectList(db.Organizations, "OrganizationID", "Name");
            var data = mobjModel.GetActivityDetail(activityid);
            Activity audituniverse = db.Activities.Find(activityid);
            ViewBag.Organizations = db.Organizations;
            ViewBag.Departemen = audituniverse.DepartementID;
            ViewBag.AuditUniverseIndex = audituniverse;
            ViewBag.audituniverse = audituniverse;

            if (ModelState.IsValid)
            {

                Activity orgObj = new Activity();
                orgObj.ActivityID = data.ActivityID;
                orgObj.ActivityParentID = data.ActivityParentID;
                orgObj.Name = data.Name;
                orgObj.Status = data.Status;
                orgObj.Status_Active = data.Status_Active;
                orgObj.Description = data.Description;

                ViewBag.IsUpdate = true;
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return View("EditAuditUniverse", orgObj);
            }
            else
                return View(data);
        }

         [HttpPost]
        public ActionResult EditAuditUniverse(string submit, [Bind(Include = "ActivityID,ActivityParentID,Name,Status,Status_Active,Tahun,Description,DepartementID")] Activity activity)
        {
            if (ModelState.IsValid)
            {
                db.Entry(activity).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Audit Universe successfully updated!";
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        [HttpPost]
        public ActionResult UpdateAuditUniverse(string submit, Activity org, string Command)
        {
            if (!ModelState.IsValid)
            {
                return View("EditAuditUniverse");
            }
            else
            {              

                Activity orgObj = new Activity();

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save" )
                    orgObj.Status = "Draft";
                else if (submit == "Send Back")
                    orgObj.Status = HelperController.GetStatusSendback(db, "Audit Universe", org.Status);
                else if (submit == "Approve")
                    orgObj.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    orgObj.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    orgObj.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {   
                                Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                                string emailContent = "Dear {0}, <BR/><BR/>Audit Universe : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Audit Universe\">link</a> to show the Audit Universe.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = baseUrl + "/AuditUniverses/Details?ActivityID=" + org.ActivityID;
                                //emailTransact.SentEmailApproval(emp.Email, emp.Name, org.Name, emailContent, url);
                        }
                    }
                }

                orgObj.ActivityID = org.ActivityID;
                orgObj.ActivityParentID = org.ActivityParentID;
                orgObj.Name = org.Name;
                //orgObj.Status = org.Status;
                orgObj.Tahun = org.Tahun;
                orgObj.Description = org.Description;
                orgObj.DepartementID = org.DepartementID;

                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                Activity oldData = db.Activities.AsNoTracking().Where(p => p.ActivityID.Equals(orgObj.ActivityID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", orgObj.ActivityID, "AuditUniverse", oldData, orgObj, username);

                bool IsSuccess = mobjModel.UpdateActivity(orgObj);
                if (IsSuccess)
                {
                    ModelState.Clear();
                    TempData["message"] = "Audit Universe successfully updated!";
                    return RedirectToAction("Index");
                }
            }

            return View("EditAuditUniverse");
        }

        [HttpGet]
         public ActionResult DeleteAuditUniverse(int activityid)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            Activity activity = db.Activities.Find(activityid);

            Activity act = new Activity();
            List<Activity> actList = db.Activities.Where(p => p.ActivityParentID == activityid).ToList();
            if (actList.Count() > 0)
                TempData["messageerror"] = "Could not delete Audit Universe due to having child!";
            else
            {
                db.Activities.Remove(activity);
                db.SaveChanges();
                auditTransact.CreateAuditTrail("Delete", activityid, "AuditUniverse", activity, act, username);
                TempData["message"] = "Audit Universe successfully deleted!";
            }
            return RedirectToAction("Index");
        }




        public ActionResult Details(int activityid)
        {
            ViewBag.DepartementID = new SelectList(db.Organizations, "OrganizationID", "Name");
            var data = mobjModel.GetActivityDetail(activityid);
            Activity audituniverse = db.Activities.Find(activityid);
            ViewBag.AuditUniverseIndex = audituniverse;
            ViewBag.audituniverse = audituniverse;
            ViewBag.Division = db.Organizations.Find(audituniverse.DepartementID).Name;

            if (!ModelState.IsValid)
            {

                Activity orgObj = new Activity();
                orgObj.ActivityID = data.ActivityID;
                orgObj.ActivityParentID = data.ActivityParentID;
                orgObj.Name = data.Name;
                orgObj.Status = data.Status;
                
                ViewBag.IsUpdate = true;
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //return View("Details", orgObj);
            }
            else
                return View(data);
        }
    }
}
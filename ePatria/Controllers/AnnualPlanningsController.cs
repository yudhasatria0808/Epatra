using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Controllers
{
    [Authorize]
    public class AnnualPlanningsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        AnnualPlanningServices mobjModel = new AnnualPlanningServices();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();


        // GET: AnnualPlannings
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var annualPlannings = db.AnnualPlannings.Include(a => a.Activity);
            return View(annualPlannings.ToList());
        }

        //GET: AnnualPlannings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnnualPlanning annualPlanning = db.AnnualPlannings.Find(id);
            if (annualPlanning == null)
            {
                return HttpNotFound();
            }

            annualPlanning.Enga = (from b in db.EngagementActivities
                              where b.ActivityID == annualPlanning.ActivityID
                              select b).ToList();
            return View(annualPlanning);
        }

        [HttpPost]
        public ActionResult GetEngagement(int nameid)
        {
            List<EngagementActivity> obj = new List<EngagementActivity>();
            obj = db.EngagementActivities.Where(m => m.ActivityID == nameid).ToList();
            SelectList obg = new SelectList(obj, "EngagementID", "Name", 0);

            return Json(obg);
            
        }

        public string GetEngagementData(int engId)
        {
            List<EngagementActivity> obj = new List<EngagementActivity>();
            var memberId = db.EngagementActivities.Find(engId).MemberID;
            var teamleaderId = db.EngagementActivities.Find(engId).TeamLeaderID;
            var supervisorId = db.EngagementActivities.Find(engId).SupervisorID;
            var picId = db.EngagementActivities.Find(engId).PICID;
            string memberName = memberId;
            if (teamleaderId == null || supervisorId == null || picId == null)
            {
                
                string teamleaderName = "";
                string supervisorName = "";
                string picName = "";
                string names =  teamleaderName + "," + supervisorName + "," + picName;
                ViewBag.datamember = memberName;
                return names + "," + memberName;
                
            }
            else
            {
                string teamleaderName = teamleaderId;
                string supervisorName = supervisorId;
                string picName = picId;
                string names =  teamleaderName + "," + supervisorName + "," + picName;
                ViewBag.datamember = memberName;
                return names +","+ memberName;

            }
            
          
        } 

        // GET: AnnualPlannings/Create
        public ActionResult Create()
        {

            //List<EngagementActivity> ci = new List<EngagementActivity> { new EngagementActivity { EngagementID = 0, ActivityID = 0 , Name = "" } };
            ActivityServices activity = new ActivityServices();
            var activities = activity.GetActivity().Where(p => p.Status == "Approve");
            IEnumerable<SelectListItem> activityid = activities
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Name
            });
            ViewBag.ActivityID = activityid;
            ViewBag.membereng = new MultiSelectList(db.Employees, "EmployeeID", "Name");
            ViewBag.membereng1 = new MultiSelectList(db.Employees, "EmployeeID", "Name");

            var a = DateTime.Now.Year + 1;
            var b = DateTime.Now.Year;

            ViewBag.Tahun = b.ToString() + " - " + a.ToString();

            ViewBag.Questioners = new SelectList(db.Questioners.Where(p => p.Status.Equals("Active")), "QuestionerID", "Name", 0);
            return View();
        }

        // POST: AnnualPlannings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string submit, [Bind(Include = "AnnualPlanningID,Date_Start,Date_End,Status,Approval_Status,Tahun,ActivityID,PICID,SupervisorID,TeamLeaderID,MemberID")] AnnualPlanning annualPlanning, string[] member, string supervisor, string pic, string leader, EngagementActivity engagementActivity)
        {
            if (ModelState.IsValid)
            {
                HttpServerUtilityBase server = Server;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    annualPlanning.Approval_Status = "Draft";
                else if (submit == "Approve")
                    annualPlanning.Approval_Status = "Approve";
                else if (submit == "Send Back")
                    annualPlanning.Approval_Status = HelperController.GetStatusSendback(db, "Annual Planning", annualPlanning.Approval_Status);
                else if (submit == "Submit For Review By" + user)
                    annualPlanning.Approval_Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    annualPlanning.Approval_Status = "Pending for Approve by" + user;
                    string activname = db.Activities.Where(p => p.ActivityID.Equals(annualPlanning.ActivityID)).Select(p => p.Name).FirstOrDefault();
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> UserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> EmpList = new List<Employee>();
                    if (UserIds.Count() > 0)
                    {
                        var users = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (UserIds.Contains(p.Id))).ToList();
                        foreach (var userr in users)
                        {

                            Employee emp = db.Employees.Where(p => p.Email.Equals(userr.Email)).FirstOrDefault();
                            if (emp != null)
                            {
                                string emailContent = "Dear {0}, <BR/><BR/>There is a Annual Planning that need your approval. Please click on this <a href=\"{2}\" title=\"Annual Planning\">link</a> to show the Annual Planning.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = baseUrl + "/AnnualPlannings/Details/" + annualPlanning.AnnualPlanningID;
                                emailTransact.SentEmailApproval(emp.Email, emp.Name, activname, emailContent, url);
                            }
                        }
                    }
                }

                db.AnnualPlannings.Add(annualPlanning);
                db.SaveChanges();
                string username = User.Identity.Name;
                AnnualPlanning an = new AnnualPlanning();
                auditTransact.CreateAuditTrail("Create", annualPlanning.AnnualPlanningID, "AnnualPlanning", an, annualPlanning, username);

                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "apl";
                rrm.Description = page + annualPlanning.AnnualPlanningID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();

                TempData["message"] = "Annual Planning successfully created!";
                return RedirectToAction("Index");
            }
            ActivityServices activity = new ActivityServices();
            var activities = activity.GetActivity().Where(p => p.Status == "Approve");
            IEnumerable<SelectListItem> activityid = activities
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Name
            });

            ViewBag.ActivityID = activityid;
            ViewBag.membereng = new MultiSelectList(db.Employees, "EmployeeID", "Name");
            ViewBag.membereng1 = new MultiSelectList(db.Employees, "EmployeeID", "Name");
            return View(annualPlanning);
        }


        [HttpPost]
        public ActionResult SaveEngagement( string engagementname,int activeid, string pic, string supervisor, string leader,string member)
        {
            EngagementActivity newEngagement = new EngagementActivity();

            newEngagement.Name = engagementname;
            newEngagement.ActivityID = activeid;
            if (pic == null || supervisor == null || leader == null )
            {
                newEngagement.MemberID = "0";
                newEngagement.PICID = "0";
                newEngagement.SupervisorID = "0";
                newEngagement.TeamLeaderID = "0";
            }
            else
            {
                newEngagement.MemberID = member;  
                newEngagement.TeamLeaderID = leader;
                newEngagement.SupervisorID = supervisor;
                newEngagement.PICID = pic;
                newEngagement.SupervisorID = supervisor;
            }
            db.EngagementActivities.Add(newEngagement);
            db.SaveChanges();
            var obj = db.EngagementActivities.Where(m => m.ActivityID == activeid).Select(p => new { EngagementID = p.EngagementID, engName = p.Name}).ToList();
            SelectList obg = new SelectList(obj, "EngagementID", "engName", 0);
            return Json(obg);
        }

        [HttpPost]
        public ActionResult UpdateEngagement(string membernull,int engId, string engagementname, int activeid, string pic, string supervisor, string leader, string member)
        {
            EngagementActivity eng = db.EngagementActivities.Find(engId);
            eng.Name = engagementname;
            if (pic == null || supervisor == null || leader == null || member == null)
            {
                eng.MemberID = "0";
                eng.PICID = "0";
                eng.SupervisorID = "0";
                eng.TeamLeaderID = "0";
            }
            else if (member == "")
            {

                eng.MemberID = member;
                eng.SupervisorID = supervisor;
                eng.TeamLeaderID = leader;
                eng.PICID = pic;
            }
            else
            {

                eng.MemberID = member;  
                eng.SupervisorID = supervisor;
                eng.TeamLeaderID = leader;
                eng.PICID = leader;
            }
            

            db.Entry(eng).State = EntityState.Modified;
            db.SaveChanges();
            var obj = db.EngagementActivities.Where(m => m.ActivityID == activeid).Select(p => new { EngagementID = p.EngagementID, engName = p.Name }).ToList();
            SelectList obg = new SelectList(obj, "EngagementID", "engName", 0);
            return Json(obg);
        }

        [HttpPost]
        public ActionResult DeleteEngagement(int engId, int activeid)
        {
            EngagementActivity engagement = db.EngagementActivities.Find(engId);
            db.EngagementActivities.Remove(engagement);
            db.SaveChanges();
            var obj = db.EngagementActivities.Where(m => m.ActivityID == activeid).Select(p => new { EngagementID = p.EngagementID, engName = p.Name }).ToList();
            SelectList obg = new SelectList(obj, "EngagementID", "engName", 0);
            return Json(obg);
        }

        // GET: AnnualPlannings/Edit/5
        public ActionResult Edit(int? id, AnnualPlanning annual)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnnualPlanning annualPlanning = db.AnnualPlannings.Find(id);
            if (annualPlanning == null)
            {
                return HttpNotFound();
            }
            ActivityServices activity = new ActivityServices();
            var activities = activity.GetActivity().Where(p => p.Status == "Approve");

            IEnumerable<SelectListItem> activityid = activities
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Name

            });
            ViewBag.ActivityID = activityid;
            ViewBag.membereng = new MultiSelectList(db.Employees, "EmployeeID", "Name");
            ViewBag.membereng1 = new MultiSelectList(db.Employees, "EmployeeID", "Name");
            ViewBag.Tahun = annualPlanning.Tahun;
            return View(annualPlanning);
        }

        // POST: AnnualPlannings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string submit, string engamentname, string supervisor, string leader, string pic, [Bind(Include = "AnnualPlanningID,Date_Start,Date_End,Status,Approval_Status,Tahun,ActivityID,PICID,SupervisorID,TeamLeaderID,MemberID")] AnnualPlanning annualPlanning, string member, EngagementActivity engagementActivity)
        {
            if (ModelState.IsValid)
            {
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    annualPlanning.Approval_Status = "Draft";
                else if (submit == "Send Back")
                    annualPlanning.Approval_Status = HelperController.GetStatusSendback(db, "Annual Planning", annualPlanning.Approval_Status);
                else if (submit == "Approve")
                    annualPlanning.Approval_Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    annualPlanning.Approval_Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    annualPlanning.Approval_Status = "Pending for Approve by" + user;
                    string activname = db.Activities.Where(p => p.ActivityID.Equals(annualPlanning.ActivityID)).Select(p => p.Name).FirstOrDefault();
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> UserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> EmpList = new List<Employee>();
                    if (UserIds.Count() > 0)
                    {
                        var users = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (UserIds.Contains(p.Id))).ToList();
                        foreach (var userr in users)
                        {

                            Employee emp = db.Employees.Where(p => p.Email.Equals(userr.Email)).FirstOrDefault();
                            if (emp != null)
                            {
                                string emailContent = "Dear {0}, <BR/><BR/>There is a Annual Planning that need your approval. Please click on this <a href=\"{2}\" title=\"Annual Planning\">link</a> to show the Annual Planning.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = baseUrl + "/AnnualPlannings/Details/" + annualPlanning.AnnualPlanningID;
                                emailTransact.SentEmailApproval(emp.Email, emp.Name, activname, emailContent, url);
                            }
                        }
                    }
                }
                db.Entry(annualPlanning).State = EntityState.Modified;
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                AnnualPlanning oldData = db.AnnualPlannings.AsNoTracking().Where(p => p.AnnualPlanningID.Equals(annualPlanning.AnnualPlanningID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", annualPlanning.AnnualPlanningID, "AnnualPlanning", oldData, annualPlanning, username);
                db.SaveChanges();
                TempData["message"] = "Annual Planning successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", annualPlanning.ActivityID);
            return View(annualPlanning);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(string submit, [Bind(Include = "AnnualPlanningID,Date_Start,Date_End,Status,Approval_Status,Tahun,ActivityID,PICID,SupervisorID,TeamLeaderID,MemberID")] AnnualPlanning annualPlanning, string member, EngagementActivity engagementActivity)
        {
            if (ModelState.IsValid)
            {
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    annualPlanning.Approval_Status = "Draft";
                else if (submit == "Send Back"){
                    annualPlanning.Approval_Status = HelperController.GetStatusSendback(db, "Annual Planning", annualPlanning.Approval_Status);
                }    
                else if (submit == "Approve")
                    annualPlanning.Approval_Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    annualPlanning.Approval_Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    annualPlanning.Approval_Status = "Pending for Approve by" + user;
                    //send email to approve user
                    string activname = db.Activities.Where(p => p.ActivityID.Equals(annualPlanning.ActivityID)).Select(p => p.Name).FirstOrDefault();
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> UserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> EmpList = new List<Employee>();
                    if (UserIds.Count() > 0)
                    {
                        var users = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (UserIds.Contains(p.Id))).ToList();
                        foreach (var userr in users)
                        {

                            Employee emp = db.Employees.Where(p => p.Email.Equals(userr.Email)).FirstOrDefault();
                            if (emp != null)
                            {
                                string emailContent = "Dear {0}, <BR/><BR/>There is a Annual Planning that need your approval. Please click on this <a href=\"{2}\" title=\"Annual Planning\">link</a> to show the Annual Planning.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = baseUrl + "/AnnualPlannings/Details/" + annualPlanning.AnnualPlanningID;
                                emailTransact.SentEmailApproval(emp.Email, emp.Name, activname, emailContent, url);
                            }
                        }
                    }
                }
                db.Entry(annualPlanning).State = EntityState.Modified;
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                AnnualPlanning oldData = db.AnnualPlannings.AsNoTracking().Where(p => p.AnnualPlanningID.Equals(annualPlanning.AnnualPlanningID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", annualPlanning.AnnualPlanningID, "AnnualPlanning", oldData, annualPlanning, username);
                db.SaveChanges();
                TempData["message"] = "Annual Planning successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", annualPlanning.ActivityID);
            return View(annualPlanning);
        }

        // GET: AnnualPlannings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnnualPlanning annualPlanning = db.AnnualPlannings.Find(id);
            if (annualPlanning == null)
            {
                return HttpNotFound();
            }
            return View(annualPlanning);
        }

        // POST: AnnualPlannings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            AnnualPlanning annualPlanning = db.AnnualPlannings.Find(id);

            AnnualPlanning emp = new AnnualPlanning();
            db.AnnualPlannings.Remove(annualPlanning);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "AnnualPlanning", annualPlanning, emp, username);
            TempData["message"] = "Annual Planning successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult Autocomplete(List<string> users, string term)
        {
            var items = users;

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AutocompleteMember(string term)
        {
            var users = HelperController.getUsersByRole("Member");
            return Autocomplete(users, term);
        }

        public ActionResult AutocompleteKetua(string term)
        {
            var users = HelperController.getUsersByRole("Ketua Tim");
            return Autocomplete(users, term);
        }

        public ActionResult AutocompletePengawas(string term)
        {
            var users = HelperController.getUsersByRole("Pengawas");
            return Autocomplete(users, term);
        }

        public ActionResult AutocompleteCIA(string term)
        {
            var users = HelperController.getUsersByRole("CIA");
            return Autocomplete(users, term);
        }

        public ActionResult AutocompleteAll(string term)
        {
            var items = db.Employees.Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

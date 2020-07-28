using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Controllers
{
    [Authorize]
    public class ExitMeetingsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        // GET: ExitMeetings
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var exitMeetings = db.ExitMeetings.Include(e => e.Activity).Include(e => e.EngagementActivity).Include(e => e.Organization).Include(e => e.LetterOfCommand);
            return View(exitMeetings.ToList());
        }

        // GET: ExitMeetings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExitMeeting exitMeeting = db.ExitMeetings.Find(id);
            if (exitMeeting == null)
            {
                return HttpNotFound();
            }

            ViewBag.nomersp = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(exitMeeting.ExitMeetingID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.activname = db.Activities.Where(p => p.ActivityID.Equals(exitMeeting.ActivityID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.enggname = db.EngagementActivities.Where(p => p.EngagementID.Equals(exitMeeting.EngagementID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.organame = db.Organizations.Where(p => p.OrganizationID.Equals(exitMeeting.OrganizationID)).Select(p => p.Name).FirstOrDefault();
           
            return View(exitMeeting);
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExitMeeting exitMeeting = await db.ExitMeetings.FindAsync(id);
            if (exitMeeting == null)
            {
                return HttpNotFound();
            }

            var nomorSP = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(exitMeeting.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.nomersp = nomorSP;
            List<FieldWork> fieldWorks = db.FieldWorks.Where(p => p.LetterOfCommand.NomorSP.Equals(nomorSP)).ToList();
            List<RCMDetailRisk> risks = new List<RCMDetailRisk>();
            List<RCMDetailRiskControl> controls = new List<RCMDetailRiskControl>();
            List<RCMDetailRiskControlIssue> issueList = new List<RCMDetailRiskControlIssue>();
            RCMDetailRiskControlIssue issue = new RCMDetailRiskControlIssue();

            //List<Employee> memberList = new List<Employee>();
            foreach (var fieldwork in fieldWorks)
            {
                risks = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(fieldwork.RiskControlMatrixID)).ToList();
                foreach (var risk in risks)
                {
                    controls = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(risk.RCMDetailRiskID)).ToList();
                    List<RCMDetailRiskControlIssue> Issues = new List<RCMDetailRiskControlIssue>();
                    foreach (var control in controls)
                    {
                        Issues = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).ToList();
                        foreach (var iss in Issues)
                        {
                            issueList.Add(iss);
                        }
                    }
                }
            }
            ViewBag.Issues = issueList;
            ViewBag.CountAllIssues = issueList.Count();
            ViewBag.CountIssueDone = issueList.Where(c=> c.Status == "Done").Count();
            ViewBag.CountIssueNotDone = issueList.Where(c => c.Status != "Done").Count();


            //List<Employee> MemberExitMeetings = new List<Employee>();
            //MemberExitMeetings = db.Employees.Where(p => p.OrganizationID.Equals(exitMeeting.OrganizationID)).ToList();
            //foreach (var member in MemberExitMeetings)
            //{
            //    memberList.Add(member);
            //}
          

            ViewBag.WordDocumentFilename = "ExitMeetings";

            return View(exitMeeting);
        }

        // GET: ExitMeetings/Create
        public ActionResult Create()
        {
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name");
            ViewBag.EngagementID = new SelectList(db.EngagementActivities, "EngagementID", "Name");
            ViewBag.OrganizationID = new SelectList(db.Organizations, "OrganizationID", "Name");
            return View();
        }

        // POST: ExitMeetings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ExitMeetingID,ActivityID,EngagementID,Date_Start,Date_End,Status,OrganizationID,LetterOfCommandID,Reviewer1,Reviewer2")] ExitMeeting exitMeeting, string activname, string enggname, string submit, string organame, string nomersp, string period1, string period2)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                IQueryable<Activity> act = db.Activities.Where(p => p.Name.Equals(activname));
                int actID = 0;
                if (act.Count() > 0 || act != null)
                    actID = act.Select(p => p.ActivityID).FirstOrDefault();
                exitMeeting.ActivityID = actID;                              

                IQueryable<EngagementActivity> eng = db.EngagementActivities.Where(p => p.Name.Equals(enggname));
                int engID = 0;
                if (eng.Count() > 0 || eng != null)
                    engID = eng.Select(p => p.EngagementID).FirstOrDefault();
                exitMeeting.EngagementID = engID;
                exitMeeting.Date_Start = Convert.ToDateTime(period1);
                exitMeeting.Date_End = Convert.ToDateTime(period2);

                IQueryable<Organization> org = db.Organizations.Where(p => p.Name.Equals(organame));
                int orgID = 0;
                if (act.Count() > 0 || act != null)
                    orgID = org.Select(p => p.OrganizationID).FirstOrDefault();
                exitMeeting.OrganizationID = orgID;

                IQueryable<LetterOfCommand> letter = db.LetterOfCommands.Where(p => p.NomorSP.Equals(nomersp));
                int letterID = 0;
                if (letter.Count() > 0 || letter != null)
                    letterID = letter.Select(p => p.LetterOfCommandID).FirstOrDefault();
                exitMeeting.LetterOfCommandID = letterID;
                LetterOfCommand sp = db.LetterOfCommands.Find(exitMeeting.LetterOfCommandID);
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    exitMeeting.Status = "Draft";
                else if (submit == "Send Back")
                    exitMeeting.Status = HelperController.GetStatusSendback(db, "Exit Meeting", exitMeeting.Status);
                else if (submit == "Approve")
                    exitMeeting.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    exitMeeting.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    exitMeeting.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = sp.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = sp.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = sp.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = sp.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                }
                exitMeeting.StatusTanggapan = "On Progress";
                db.ExitMeetings.Add(exitMeeting);
                db.SaveChanges();
                ExitMeeting exitmeet = new ExitMeeting();
                auditTransact.CreateAuditTrail("Create", exitMeeting.ExitMeetingID, "Exit Meeting", exitmeet, exitMeeting, username);

                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "exitmeet";
                rrm.Description = page + exitMeeting.ExitMeetingID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();

                TempData["message"] = "Exit Meeting successfully created!";
                return RedirectToAction("Index");
            }

            //LetterOfCommand letter = db.LetterOfCommands.Where(p => p.NomorSP.Equals(nomersp)).FirstOrDefault();
            //var activId = letter.ActivityID;
            //var engId = letter.Preliminary.EngagementID;
            //var orgId = letter.Activity.DepartementID;
            //string activnam = db.Activities.Find(activId).Name;
            //string engname = db.EngagementActivities.Find(engId).Name;
            //string orgname = db.Organizations.Find(orgId).Name;
            //var data =  activnam + "," + engname + "," + orgname;
            return View(exitMeeting);
        }

        // GET: ExitMeetings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExitMeeting exitMeeting = db.ExitMeetings.Find(id);
            if (exitMeeting == null)
            {
                return HttpNotFound();
            }
            var nomorSP = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(exitMeeting.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.nomersp = nomorSP;
            ViewBag.activname = db.Activities.Where(p => p.ActivityID.Equals(exitMeeting.ActivityID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.enggname = db.EngagementActivities.Where(p => p.EngagementID.Equals(exitMeeting.EngagementID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.organame = db.Organizations.Where(p => p.OrganizationID.Equals(exitMeeting.OrganizationID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.datestart = exitMeeting.Date_Start.Value.ToString("dd/MM/yyyy HH:mm");
            ViewBag.dateend = exitMeeting.Date_End.Value.ToString("dd/MM/yyyy HH:mm");
            List<FieldWork> fieldWorks = db.FieldWorks.Where(p => p.LetterOfCommand.NomorSP.Equals(nomorSP)).ToList();
            List<RCMDetailRisk> risks = new List<RCMDetailRisk>();
            List<RCMDetailRiskControl> controls = new List<RCMDetailRiskControl>();
            List<RCMDetailRiskControlIssue> issueList = new List<RCMDetailRiskControlIssue>();
            RCMDetailRiskControlIssue issue = new RCMDetailRiskControlIssue();
            foreach (var fieldwork in fieldWorks)
            {
                risks = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(fieldwork.RiskControlMatrixID)).ToList();
                foreach (var risk in risks)
                {
                    controls = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(risk.RCMDetailRiskID)).ToList();
                    List<RCMDetailRiskControlIssue> Issues = new List<RCMDetailRiskControlIssue>();
                    foreach (var control in controls)
                    {
                        //var singleIssues = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).OrderByDescending(d => d.RCMDetailRiskControlIssueID).FirstOrDefault();
                        //issueList.Add(singleIssues);

                        Issues = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).ToList();
                        foreach (var iss in Issues)
                        {
                            issueList.Add(iss);
                        }
                    }
                }
            }
            ViewBag.Issues = issueList;
            return View(exitMeeting);
        }

        private bool sentEmailEM(ExitMeeting exitMeeting, string user)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
            List<Employee> CIAEmpList = new List<Employee>();
            string SPNo = db.LetterOfCommands.Find(exitMeeting.LetterOfCommandID).NomorSP;
            if (CIAUserIds.Count() > 0)
            {
                var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                foreach (var CIAUser in CIAUsers)
                {
                    Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();
                    if (emp != null)
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>Exit Meeting for SP : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Exit Meeting\">link</a> to show the Exit Meeting.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/ExitMeetings/Edit/" + exitMeeting.ExitMeetingID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, SPNo, emailContent, url);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailEM(string userToSentEmail, ExitMeeting exitMeeting)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            string SPNo = db.LetterOfCommands.Find(exitMeeting.LetterOfCommandID).NomorSP;
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>Exit Meeting for SP : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Exit Meeting\">link</a> to show the Exit Meeting.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = baseUrl + "/ExitMeetings/Edit/" + exitMeeting.ExitMeetingID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, SPNo, emailContent, url);
            }
            return true;
        }

        // POST: ExitMeetings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ExitMeetingID,ActivityID,EngagementID,Date_Start,Date_End,Status,OrganizationID,LetterOfCommandID,Reviewer1,Reviewer2,StatusTanggapan")] ExitMeeting exitMeeting,string activname, string enggname, string submit, string organame, string nomersp, string period1, string period2)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                IQueryable<Activity> act = db.Activities.Where(p => p.Name.Equals(activname));
                int actID = 0;
                if (act.Count() > 0 || act != null)
                    actID = act.Select(p => p.ActivityID).FirstOrDefault();
                exitMeeting.ActivityID = actID;

                IQueryable<EngagementActivity> eng = db.EngagementActivities.Where(p => p.Name.Equals(enggname));
                int engID = 0;
                if (eng.Count() > 0 || eng != null)
                    engID = eng.Select(p => p.EngagementID).FirstOrDefault();
                exitMeeting.EngagementID = engID;
                exitMeeting.Date_Start = Convert.ToDateTime(period1);
                exitMeeting.Date_End = Convert.ToDateTime(period2);

                IQueryable<Organization> org = db.Organizations.Where(p => p.Name.Equals(organame));
                int orgID = 0;
                if (act.Count() > 0 || act != null)
                    orgID = org.Select(p => p.OrganizationID).FirstOrDefault();
                exitMeeting.OrganizationID = orgID;

                IQueryable<LetterOfCommand> letter = db.LetterOfCommands.Where(p => p.NomorSP.Equals(nomersp));
                int letterID = 0;
                if (letter.Count() > 0 || letter != null)
                    letterID = letter.Select(p => p.LetterOfCommandID).FirstOrDefault();
                exitMeeting.LetterOfCommandID = letterID;
                LetterOfCommand sp = db.LetterOfCommands.Find(exitMeeting.LetterOfCommandID);

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    exitMeeting.Status = "Draft";
                else if(submit == "Send Back")
                    exitMeeting.Status = HelperController.GetStatusSendback(db, "Exit Meeting", exitMeeting.Status);
                else if (submit == "Approve")
                {
                    exitMeeting.Status = "Approve";
                    exitMeeting.StatusTanggapan = "Closed";
                }
                else if (submit == "Submit For Review By" + user)
                    exitMeeting.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    exitMeeting.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = sp.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = sp.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = sp.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = sp.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailEM(userToSentEmail, exitMeeting);
                        else
                            sentEmailEM(exitMeeting, user.Trim());
                    }
                }
                ExitMeeting oldData = db.ExitMeetings.AsNoTracking().Where(p => p.ExitMeetingID.Equals(exitMeeting.ExitMeetingID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", exitMeeting.ExitMeetingID, "Exit Meeting", oldData, exitMeeting, username);
                db.Entry(exitMeeting).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Exit Meeting successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.nomersp = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(exitMeeting.ExitMeetingID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.activname = db.Activities.Where(p => p.ActivityID.Equals(exitMeeting.ActivityID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.enggname = db.EngagementActivities.Where(p => p.EngagementID.Equals(exitMeeting.EngagementID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.organame = db.Organizations.Where(p => p.OrganizationID.Equals(exitMeeting.OrganizationID)).Select(p => p.Name).FirstOrDefault();
            return View(exitMeeting);
        }

        // GET: ExitMeetings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExitMeeting exitMeeting = db.ExitMeetings.Find(id);
            if (exitMeeting == null)
            {
                return HttpNotFound();
            }
            return View(exitMeeting);
        }

        // POST: ExitMeetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ExitMeeting exitMeeting = db.ExitMeetings.Find(id);
            ExitMeeting exitmeet = new ExitMeeting();
            db.ExitMeetings.Remove(exitMeeting);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Exit Meeting", exitMeeting, exitmeet, username);
            TempData["message"] = "Exit Meeting successfully deleted!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateStatus(int ExitMeetingID,string submit)
        {
            var Exit = db.ExitMeetings.Find(ExitMeetingID);
            switch (submit)
            {
                case "Approve":
                    Exit.Status = "Approve";
                    break;
                case "Submit for Review":
                    Exit.Status = "Pending for Approval";
                    break;

                default:
                    //throw new Exception();
                    break;
            }
            db.Entry(Exit).State = EntityState.Modified;
            db.SaveChanges();
            TempData["message"] = "Exit Meeting successfully updated!";
            return RedirectToAction("Index");
        }

        public string GetData(string nomer)
        {
            LetterOfCommand letter = db.LetterOfCommands.Where(p => p.NomorSP.Equals(nomer)).FirstOrDefault();
            int activId = 0;
            int? engId = 0;
            int? orgId = 0;
            string activname = String.Empty;
            string engname = String.Empty;
            string orgname = String.Empty;
            if (letter != null)
            {
                activId = letter.ActivityID;
                engId = letter.AuditPlanningMemorandum.Preliminary.EngagementID;
                orgId = letter.Activity.DepartementID;
                activname = db.Activities.Find(activId) != null ? db.Activities.Find(activId).Name : String.Empty;
                engname = db.EngagementActivities.Find(engId) != null ? db.EngagementActivities.Find(engId).Name : String.Empty;
                orgname = db.Organizations.Find(orgId) != null ? db.Organizations.Find(orgId).Name : String.Empty;
            }
            return activname + "," + engname + "," + orgname;
        }

        public ActionResult GetIssue(string nomer)
        {
            List<FieldWork> fieldWorks = db.FieldWorks.Where(p => p.LetterOfCommand.NomorSP.Equals(nomer)).ToList();
            List<RCMDetailRisk> risks = new List<RCMDetailRisk>();
            List<RCMDetailRiskControl> controls = new List<RCMDetailRiskControl>();
            List<RCMDetailRiskControlIssue> issueList = new List<RCMDetailRiskControlIssue>();
            RCMDetailRiskControlIssue issue = new RCMDetailRiskControlIssue();
            foreach (var fieldwork in fieldWorks)
            {
                risks = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(fieldwork.RiskControlMatrixID)).ToList();
                foreach (var risk in risks)
                {
                    controls = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(risk.RCMDetailRiskID)).ToList();
                    List<RCMDetailRiskControlIssue> Issues = new List<RCMDetailRiskControlIssue>();
                    foreach (var control in controls)
                    {
                        //var singleIssues = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).OrderByDescending(d=>d.RCMDetailRiskControlIssueID).FirstOrDefault();
                        //issueList.Add(singleIssues);
                        Issues = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).ToList();
                        foreach (var iss in Issues)
                        {
                            issueList.Add(iss);
                        }
                    }
                }
            }
            db.Configuration.ProxyCreationEnabled = false;
            return Json(new { issueList = issueList }, JsonRequestBehavior.AllowGet);
        }

        public bool UpdateIssueSign(string issueNo, string signification)
        {
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Where(p => p.NoRef.Equals(issueNo)).FirstOrDefault();
            issue.Signification = signification;
            db.Entry(issue).State = EntityState.Modified;
            db.SaveChanges();
            return true;

        }

        public ActionResult EditManagementResponse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Find(id);
            ViewBag.picName = issue.PICID != null ? issue.PICID : null;
            return View(issue);
        }

        [ValidateInput(false)]
        public bool UpdateManagementResponse(int issueID, string managementResponse, string issueStatus, string dueDate, string picName, int emId)
        {
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Find(issueID);
            issue.ManagementResponse = managementResponse;
            issue.Status = issueStatus;
            issue.Due_Date = dueDate;
            issue.PICID = picName != String.Empty ? db.Employees.Where(p => p.Name.Equals(picName)).FirstOrDefault().Name : null;
            db.Entry(issue).State = EntityState.Modified;
            if (issueStatus == "Agree")
            {
                FieldWork fw = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(issue.RCMDetailRiskControl.RCMDetailRisk.RiskControlMatrixID)).FirstOrDefault();
                MonitoringTindakLanjut mon1 = db.MonitoringTindakLanjut.Where(d => d.RCMDetailRiskControlIssueID == issue.RCMDetailRiskControlIssueID && d.LetterOfCommandID == fw.LetterOfCommandID).FirstOrDefault();
                if (mon1 != null)
                    db.MonitoringTindakLanjut.Remove(mon1);

                MonitoringTindakLanjut mon = new MonitoringTindakLanjut();
                mon.RCMDetailRiskControlIssueID = issue.RCMDetailRiskControlIssueID;
                mon.Status = "Open";
                mon.LetterOfCommandID = fw.LetterOfCommandID;
                db.MonitoringTindakLanjut.Add(mon);
            }
            else
            {
                FieldWork fw = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(issue.RCMDetailRiskControl.RCMDetailRisk.RiskControlMatrixID)).FirstOrDefault();
                MonitoringTindakLanjut mon = db.MonitoringTindakLanjut.Where(d => d.RCMDetailRiskControlIssueID == issue.RCMDetailRiskControlIssueID && d.LetterOfCommandID == fw.LetterOfCommandID).FirstOrDefault();
                if (mon != null)
                    db.MonitoringTindakLanjut.Remove(mon);
            }
            ExitMeeting em = db.ExitMeetings.Find(emId);
            //em.StatusTanggapan = "Closed";
            db.Entry(em).State = EntityState.Modified;
            db.SaveChanges();
            return true;
        }

        public ActionResult Autocomplete(string term)
        {
            var items = db.FieldWorks.Where(p => p.Status.Equals("Approve")).Select(p => p.LetterOfCommand.NomorSP).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PICAutocomplete(string term)
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

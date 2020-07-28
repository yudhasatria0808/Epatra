using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ePatria.Security;

namespace ePatria.Controllers
{
    //[Authorize()]
    [FieldWorkAuth()]
    public class FieldWorksController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private FilesUploadController filesTransact = new FilesUploadController();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private HelperController helperTransact = new HelperController();
        // GET: FieldWorks
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            string messageerror = TempData["messageerror"] as string;
            if (!string.IsNullOrEmpty(message)) 
            {
                ViewBag.message = message;
            } else if (!string.IsNullOrEmpty(messageerror))
            {
                ViewBag.messageerror = messageerror;
            }
            //User.Identity.Name
            string name =  HelperController.getNameByUserName(User.Identity.Name);
            var fieldWorks = db.FieldWorks.Include(f => f.LetterOfCommand).Include(f => f.RiskControlMatrix).ToList();

            if (HelperController.isSuperAdmin(User.Identity.GetUserId()))
                return View(fieldWorks);
            else
                return View(fieldWorks.Where(d => d.LetterOfCommand.SupervisorID == name || d.LetterOfCommand.TeamLeaderID == name || d.LetterOfCommand.PICID == name  || d.LetterOfCommand.MemberID.Split(';').Contains(name)));
        }

        // GET: FieldWorks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FieldWork fieldWork = await db.FieldWorks.FindAsync(id);
            if (fieldWork == null)
            {
                return HttpNotFound();
            }
            return View(fieldWork);
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuditQuery auditQuery = await db.AuditQuery.FindAsync(id);
            if (auditQuery == null)
            {
                return HttpNotFound();
            }

            ViewBag.Kepada = auditQuery.Kepada; //.Replace(";", "<br />:&nbsp;");

            IQueryable<Employee> emp = db.Employees.Where(p => p.EmployeeID.Equals(auditQuery.Dari));

            ViewBag.dari = emp.Select(p => p.Name).FirstOrDefault();

            int a = auditQuery.RCMDetailRiskControlDetail.RCMDetailRiskControl.RCMDetailRisk.RiskControlMatrixID;
            IQueryable<FieldWork> sp_print = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(a));

            ViewBag.spPrint = sp_print.Select(p => p.LetterOfCommand.NomorSP).FirstOrDefault();
            ViewBag.WordDocumentFilename = "AuditQuery";
            return View(auditQuery);
        }

        // GET: FieldWorks/Create
        public ActionResult Create()
        {
            ViewBag.LetterOfCommandID = new SelectList(db.LetterOfCommands, "LetterOfCommandID", "NomorSP");
            //ViewBag.RiskControlMatrixID = new SelectList(db.RiskControlMatrixs.Where(p => p.Status.Equals("Approve")), "RiskControlMatrixID", "Objectives");
            //ViewBag.RiskControlMatrixID = new SelectList();
            return View();
        }

        // POST: FieldWorks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FieldWorkID,LetterOfCommandID,RiskControlMatrixID,Status")] FieldWork fieldWork, string submit)
        {
            if (ModelState.IsValid)
            {
                LetterOfCommand sp = db.LetterOfCommands.Find(fieldWork.LetterOfCommandID);
                string username = User.Identity.Name;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    fieldWork.Status = "Draft";
                else if (submit == "Send Back")
                    fieldWork.Status = HelperController.GetStatusSendback(db, "FieldWork", fieldWork.Status);
                else if (submit == "Submit For Review By" + user)
                {
                        fieldWork.Status = "Pending for Review by" + user;
                }
                else if (submit == "Submit For Approve By" + user)
                {
                        fieldWork.Status = "Pending for Approve by" + user;

                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = sp.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = sp.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = sp.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = sp.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                }
                db.FieldWorks.Add(fieldWork);
                await db.SaveChangesAsync();
                FieldWork fwork = new FieldWork();
                auditTransact.CreateAuditTrail("Create", fieldWork.FieldWorkID, "Field Work", fwork, fieldWork, username);

                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "fieldwork";
                rrm.Description = page + fieldWork.FieldWorkID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();

                TempData["message"] = "Field Work successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.LetterOfCommandID = new SelectList(db.LetterOfCommands, "LetterOfCommandID", "NomorSP", fieldWork.LetterOfCommandID);
            ViewBag.RiskControlMatrixID = new SelectList(db.RiskControlMatrixs, "RiskControlMatrixID", "Objectives", fieldWork.RiskControlMatrixID);
            return View(fieldWork);
        }

        // GET: FieldWorks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FieldWork fieldWork = await db.FieldWorks.FindAsync(id);
            if (fieldWork == null)
            {
                return HttpNotFound();
            }
            ViewBag.LetterOfCommandID = new SelectList(db.LetterOfCommands, "LetterOfCommandID", "NomorSP", fieldWork.LetterOfCommandID);
            ViewBag.RiskControlMatrixID = new SelectList(db.RiskControlMatrixs, "RiskControlMatrixID", "BusinessProcesID", fieldWork.RiskControlMatrixID);
            RiskControlMatrix rcm = db.RiskControlMatrixs.Find(fieldWork.RiskControlMatrixID);
            ViewBag.BPM = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProcesID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.SubBP = rcm.SubBusinessProcess;
            ViewBag.Obj = rcm.Objectives;
            ViewBag.RiskControlMatrixID = fieldWork.RiskControlMatrixID;
            ViewBag.LetterOfCommandID = fieldWork.LetterOfCommandID;
            var riskList = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcm.RiskControlMatrixID).Select(p => p.RCMDetailRiskID).ToList();
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcm.RiskControlMatrixID).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            var allControl = db.RCMDetailRiskControls.Where(p => riskList.Contains(p.RCMDetailRiskID)).ToList();
            var allNotPendingControl = allControl.Where(p => !p.Status.Equals("Pending")).ToList();
            double controlCount = ((double)allNotPendingControl.Count() / (double)allControl.Count()) * 100;
            ViewBag.ControlCount = controlCount;
            var allReviewedControl = allControl.Where(p => !String.IsNullOrEmpty(p.ReviewStatus)).ToList();
            double reviewedCount = ((double)allReviewedControl.Count() / (double)allControl.Count()) * 100;
            ViewBag.reviewedCount = reviewedCount;
            ViewBag.RDR = new SelectList(rdr, "riskId", "riskName");


            List<FieldWork> fieldWorks = db.FieldWorks.Where(p => p.FieldWorkID.Equals(fieldWork.FieldWorkID)).ToList();
            List<RCMDetailRisk> risks = new List<RCMDetailRisk>();
            List<RCMDetailRiskControl> controls = new List<RCMDetailRiskControl>();
            List<RCMDetailRiskControlIssue> issueList = new List<RCMDetailRiskControlIssue>();
            List<AuditQuery> auditQueryList = new List<AuditQuery>();
            RCMDetailRiskControlIssue issue = new RCMDetailRiskControlIssue();
            foreach (var fieldwork in fieldWorks)
            {
                risks = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(fieldwork.RiskControlMatrixID)).ToList();
                foreach (var risk in risks)
                {
                    controls = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(risk.RCMDetailRiskID)).ToList();
                    //List<RCMDetailRiskControlIssue> Issues = new List<RCMDetailRiskControlIssue>();
                    foreach (var control in controls)
                    {
                        List<RCMDetailRiskControlIssue> Issues = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).ToList();
                        foreach (var iss in Issues)
                        {
                            iss.Fact = System.Text.RegularExpressions.Regex.Replace(iss.Fact, @"<[^>]*>", String.Empty);
                            issueList.Add(iss);
                            
                        }
                        List<AuditQuery> AuditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetail.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).ToList();
                        foreach (var item in AuditQuery)
                        {
                            //iss.Fact = System.Text.RegularExpressions.Regex.Replace(iss.Fact, @"<[^>]*>", String.Empty);
                            auditQueryList.Add(item);
                        }
                    }
                }
            }

            
            ViewBag.Issues = issueList;
            ViewBag.AuditQuery = auditQueryList;
           
            return View(fieldWork);
        }

        public ActionResult EditStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Find(id);
            ViewBag.Status_Approval = issue.Status_Approval;
            return View(issue);
        }

        [ValidateInput(false)]
        public bool UpdateStatusIssue(int issueID, string status,int id)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Find(issueID);
            
            issue.Status_Approval = status;
            RCMDetailRiskControlIssue oldData = db.RCMDetailRiskControlIssue.AsNoTracking().Where(p => p.RCMDetailRiskControlIssueID.Equals(issue.RCMDetailRiskControlIssueID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", issue.RCMDetailRiskControlIssueID, "Status Issue", oldData, issue, username);
            db.Entry(issue).State = EntityState.Modified;
            db.SaveChanges();

            if (issue.Status_Approval == "Review")
            {
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals("CIA")).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                List<Employee> CIAEmpList = new List<Employee>();
                if (CIAUserIds.Count() > 0)
                {
                    var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                    foreach (var CIAUser in CIAUsers)
                    {
                        if (CIAUser.FirstName != "ePatria")
                        {
                            Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                            string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as CIA for Issue : {1}. Someone has approve on this Issue. Please click on this <a href=\"{2}\" title=\"your Issue\">link</a> to show the Issue.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                            string url = baseUrl + "/FieldWorks/Edit/" + id;
                            emailTransact.SentEmailApproval(emp.Email, emp.Name, issue.Title, emailContent, url);

                        }
                    }
                }
            }
            return true;
        }

        private bool sentEmailFW(FieldWork fieldWork, string user)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
            List<Employee> CIAEmpList = new List<Employee>();
            var no = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(fieldWork.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            if (CIAUserIds.Count() > 0)
            {
                var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                foreach (var CIAUser in CIAUsers)
                {
                    Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();
                    if (emp != null)
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>Fieldwork for SP : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Fieldwork\">link</a> to show the Fieldwork.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/FieldWorks/Edit/" + fieldWork.FieldWorkID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, no, emailContent, url);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailFW(string userToSentEmail, FieldWork fieldWork)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            var no = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(fieldWork.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>Fieldwork for SP : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Fieldwork\">link</a> to show the Fieldwork.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = baseUrl + "/FieldWorks/Edit/" + fieldWork.FieldWorkID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, no, emailContent, url);
            }
            return true;
        }

        // POST: FieldWorks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FieldWorkID,LetterOfCommandID,RiskControlMatrixID,Status")] FieldWork fieldWork, string submit)
        {
            if (ModelState.IsValid)
            {
                LetterOfCommand sp = db.LetterOfCommands.Find(fieldWork.LetterOfCommandID);
				string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    fieldWork.Status = "Draft";
                else if(submit == "Send Back")
                    fieldWork.Status = HelperController.GetStatusSendback(db, "FieldWork", fieldWork.Status);
                else if (submit == "Approve")
                {
                    if (checkCanApprove(fieldWork.FieldWorkID))
                    {
                        fieldWork.Status = "Approve";
                    }
                    else
                    {
                        TempData["messageerror"] = "Cannot Approve This FieldWork Because Still Have Audit Queries or Issues That Not Yet Approved";
                        return RedirectToAction("Index");
                    }
                }
                else if (submit == "Submit For Review By" + user)
                {
                    if (checkCanApprove(fieldWork.FieldWorkID))
                    {
                        fieldWork.Status = "Pending for Review by" + user;
                    }
                    else
                    {
                        TempData["messageerror"] = "Cannot Submit This FieldWork Because Still Have Audit Queries or Issues That Not Yet Approved";
                        return RedirectToAction("Index");
                    }
                }
                else if (submit == "Submit For Approve By" + user)
                {
                    if (checkCanApprove(fieldWork.FieldWorkID))
                    {
                        fieldWork.Status = "Pending for Approve by" + user;
                    }
                    else
                    {
                        TempData["messageerror"] = "Cannot Submit This FieldWork Because Still Have Audit Queries or Issues That Not Yet Approved";
                        return RedirectToAction("Index");
                    }

                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = sp.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = sp.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = sp.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = sp.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailFW(userToSentEmail, fieldWork);
                        else
                            sentEmailFW(fieldWork, user.Trim());
                    }
                }
                FieldWork oldData = db.FieldWorks.AsNoTracking().Where(p => p.FieldWorkID.Equals(fieldWork.FieldWorkID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", fieldWork.FieldWorkID, "Field Work", oldData, fieldWork, username);
                db.Entry(fieldWork).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (fieldWork.Status == "Approve")
                {
                    Reporting newReporting = new Reporting();
                    newReporting.LetterOfCommandID = fieldWork.LetterOfCommandID;
                    newReporting.FieldWorkID = fieldWork.FieldWorkID;
                    newReporting.NomorLaporan = helperTransact.getNewNumber("Reporting");
                    db.Reportings.Add(newReporting);
                    Feedback newFeedback = new Feedback();
                    newFeedback.FieldWorkID = fieldWork.FieldWorkID;
                    db.Feedbacks.Add(newFeedback);
                    db.SaveChanges();
                    
                }
                TempData["message"] = "Field Work successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.LetterOfCommandID = new SelectList(db.LetterOfCommands, "LetterOfCommandID", "NomorSP", fieldWork.LetterOfCommandID);
            ViewBag.RiskControlMatrixID = new SelectList(db.RiskControlMatrixs, "RiskControlMatrixID", "Objectives", fieldWork.RiskControlMatrixID);
            return View(fieldWork);
        }

        // GET: FieldWorks/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FieldWork fieldWork = await db.FieldWorks.FindAsync(id);
            if (fieldWork == null)
            {
                return HttpNotFound();
            }
            return View(fieldWork);
        }

        // POST: FieldWorks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            FieldWork fieldWork = await db.FieldWorks.FindAsync(id);
            FieldWork fwork = new FieldWork();
            db.FieldWorks.Remove(fieldWork);
            await db.SaveChangesAsync();
            auditTransact.CreateAuditTrail("Delete", id, "Field Work", fieldWork, fwork, username);
            TempData["message"] = "Field Work successfully deleted!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region RCMTransact

        [HttpPost]
        public ActionResult GetControlsByRiskId(int riskId)
        {
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "controlName", 0);
            return Json(riskControlSelectList);
        }

        public int GetCountStatusByRiskId(int riskId)
        {
            var controlList = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(riskId) && !p.Status.Equals("Pending")).ToList();
            return controlList.Count();
        }

        public int GetCountReviewByRiskId(int riskId)
        {
            var controlList = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(riskId) && !String.IsNullOrEmpty(p.ReviewStatus)).ToList();
            return controlList.Count();
        }

        public string GetAllStatusAndReviewControl(int rcmId)
        {
            var riskList = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(rcmId)).Select(p => p.RCMDetailRiskID).ToList();
            var allControl = db.RCMDetailRiskControls.Where(p => riskList.Contains(p.RCMDetailRiskID)).ToList();
            var allNotPendingControl = allControl.Where(p => !p.Status.Equals("Pending")).ToList();
            double controlCount = ((double)allNotPendingControl.Count() / (double)allControl.Count()) * 100;
            var allReviewedControl = allControl.Where(p => !String.IsNullOrEmpty(p.ReviewStatus)).ToList();
            double reviewedCount = ((double)allReviewedControl.Count() / (double)allControl.Count()) * 100;
            string result = Convert.ToString(controlCount) + ";" + Convert.ToString(reviewedCount);
            return result;
        }


        [HttpPost]
        public ActionResult GetAuditStepByRiskControlId(int controlId)
        {
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult SaveRisk(int rcmId, string riskName)
        {
            string username = User.Identity.Name;
            RCMDetailRisk newRdr = new RCMDetailRisk();
            newRdr.RiskControlMatrixID = rcmId;
            newRdr.RiskName = riskName;
            newRdr.Status = "Pending";
            db.RCMDetailRisks.Add(newRdr);
            db.SaveChanges();
            RCMDetailRisk createrisk = new RCMDetailRisk();
            auditTransact.CreateAuditTrail("Create", newRdr.RCMDetailRiskID, "Risk", createrisk, newRdr, username);
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult UpdateRisk(int rcmId, int riskId, string riskName)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRisk Rdr = db.RCMDetailRisks.Find(riskId);
            Rdr.RiskName = riskName;
            RCMDetailRisk oldData = db.RCMDetailRisks.AsNoTracking().Where(p => p.RCMDetailRiskID.Equals(Rdr.RCMDetailRiskID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", Rdr.RCMDetailRiskID, "Risk", oldData, Rdr, username);
            db.Entry(Rdr).State = EntityState.Modified;
            db.SaveChanges();
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult DeleteRisk(int rcmId, int riskId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            RCMDetailRisk Rdr = db.RCMDetailRisks.Find(riskId);
            RCMDetailRisk deleterisk = new RCMDetailRisk();
            db.RCMDetailRisks.Remove(Rdr);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", riskId, "Risk", Rdr, deleterisk, username);
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult SaveControl(int riskId, string controlName)
        {
            string username = User.Identity.Name;
            RCMDetailRiskControl newControl = new RCMDetailRiskControl();
            newControl.RCMDetailRiskID = riskId;
            newControl.ControlName = controlName;
            newControl.ReviewMasterID = 0;
            newControl.DueDate = DateTime.Now;
            newControl.CloseDate = DateTime.Now;
            newControl.Status = "Pending";
            db.RCMDetailRiskControls.Add(newControl);
            db.SaveChanges();
            RCMDetailRiskControl createcontrol = new RCMDetailRiskControl();
            auditTransact.CreateAuditTrail("Create", newControl.RCMDetailRiskControlID, "Control", createcontrol, newControl, username);
            ReviewRelationMaster rrm = new ReviewRelationMaster();
            string page = "control";
            rrm.Description = page + newControl.RCMDetailRiskControlID;
            db.ReviewRelationMasters.Add(rrm);
            db.SaveChanges();
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult UpdateControl(int riskId, int controlId, string controlName)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
            control.ControlName = controlName;
            RCMDetailRiskControl oldData = db.RCMDetailRiskControls.AsNoTracking().Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", control.RCMDetailRiskControlID, "Control", oldData, control, username);
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult SetControlStatus(int riskId, int controlId, string controlStatus)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
            control.Status = controlStatus;
            RCMDetailRiskControl oldData = db.RCMDetailRiskControls.AsNoTracking().Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", control.RCMDetailRiskControlID, "Control Status", oldData, control, username);
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "controlName", 0);
            return Json(riskControlSelectList);
        }
        
        [HttpPost]
        public ActionResult GetControlStatus(int riskId, int controlId, string controlStatus)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
            control.Status = controlStatus;
            RCMDetailRiskControl oldData = db.RCMDetailRiskControls.AsNoTracking().Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", control.RCMDetailRiskControlID, "Control Status", oldData, control, username);
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "controlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult SetControlReviewStatus(int riskId, int controlId, string controlReviewStatus)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
            control.ReviewStatus = controlReviewStatus;
            RCMDetailRiskControl oldData = db.RCMDetailRiskControls.AsNoTracking().Where(p => p.RCMDetailRiskControlID.Equals(control.RCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", control.RCMDetailRiskControlID, "Control Riview Status", oldData, control, username);
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "controlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult DeleteControl(int riskId, int controlId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            RCMDetailRiskControlDetail cdetail = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            RCMDetailRiskControlIssue cIssue = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            if (cdetail == null && cIssue == null)
            {
                RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
                RCMDetailRiskControl deletecontrol = new RCMDetailRiskControl();
                db.RCMDetailRiskControls.Remove(control);
                db.SaveChanges();
                auditTransact.CreateAuditTrail("Delete", controlId, "Control", control, deletecontrol, username);
            }
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult SaveAudit(int controlId, string auditName)
        {
            string username = User.Identity.Name;
            RCMDetailControlAuditStep newAudit = new RCMDetailControlAuditStep();
            newAudit.RCMDetailRiskControlID = controlId;
            newAudit.AuditStepName = auditName;
            newAudit.Status = "Pending";
            db.RCMDetailControlAuditSteps.Add(newAudit);
            db.SaveChanges();
            RCMDetailControlAuditStep createaudit = new RCMDetailControlAuditStep();
            auditTransact.CreateAuditTrail("Create", newAudit.RCMDetailControlAuditStepID, "Audit", createaudit, newAudit, username);
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList();
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult UpdateAudit(int controlId,  string auditName)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailControlAuditStep audit = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            audit.AuditStepName = auditName;
            RCMDetailControlAuditStep oldData = db.RCMDetailControlAuditSteps.AsNoTracking().Where(p => p.RCMDetailControlAuditStepID.Equals(audit.RCMDetailControlAuditStepID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", audit.RCMDetailControlAuditStepID, "Audit", oldData, audit, username);
            db.Entry(audit).State = EntityState.Modified;
            db.SaveChanges();
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList();
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult DeleteAudit(int controlId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            RCMDetailControlAuditStep audit = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            RCMDetailControlAuditStep deleteaudit = new RCMDetailControlAuditStep();
            db.RCMDetailControlAuditSteps.Remove(audit);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", controlId, "Audit", audit, deleteaudit, username);
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateAuditQuery(string no, string tanggal, string kepada,string status, string dari, string lampiran, string perihal, string pembuka, string issuedesc, string criteria, string impact, string auditeeclar, string penutup, string folder, int controlId)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            AuditQuery aq = db.AuditQuery.Where(p => p.No.Equals(no)).FirstOrDefault();

            int i = 0;
            HttpServerUtilityBase server = Server;
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            List<string> keepImages = new List<string>();
            //object imagesTemp = Session["Images"];
            var sessionImages = Session["Images"];
            if (sessionImages != null)
            {
                Array arrKeepImages = (Array)sessionImages;
                foreach (var arrKeepImage in arrKeepImages)
                    keepImages.Add(arrKeepImage.ToString());
                var noid = aq.No.Split('/')[0];
                var aqname = aq.No.Split('/')[1];
                var noaq = aq.No.Split('/')[2];
                bool getFiles = filesTransact.getFiles(noid + aqname + noaq, out newFilesName, out paths, url, server);
                List<string> deletedFiles = paths.Except(keepImages).ToList();
                bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                i = filesTransact.getLastNumberOfFiles(noid + aqname + noaq, server);
            }

            Session.Remove("Images");

            //int kepadaId = db.Employees.Where(p => p.Name == kepada).Select(p => p.EmployeeID).FirstOrDefault();
            int dariId = db.Employees.Where(p => p.Name == dari).Select(p => p.EmployeeID).FirstOrDefault();
            aq.No = no; aq.Tanggal = (tanggal != String.Empty) ? Convert.ToDateTime(tanggal) : DateTime.Now; aq.Kepada = kepada; aq.Dari = dariId; aq.Lampiran = lampiran;
            aq.Perihal = perihal; aq.Pembuka = pembuka; aq.IssueDesc = issuedesc; aq.Criteria = criteria; aq.Impact = impact;
            aq.AuditeeClarification = auditeeclar; aq.Penutup = penutup; aq.Folder = folder;
            AuditQuery oldData = db.AuditQuery.AsNoTracking().Where(p => p.AuditQueryID.Equals(aq.AuditQueryID)).FirstOrDefault();
            //aq.Status = status;
            if (status.ToLower().Contains("approve"))
                aq.Status = status;
            else
                aq.Status = HelperController.GetStatusSendback(db, "Audit Query", aq.Status);
            auditTransact.CreateAuditTrail("Update", aq.AuditQueryID, "Audit Query", oldData, aq, username);
            db.Entry(aq).State = EntityState.Modified;
            db.SaveChanges();

            var controlDetail = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            var auditQuery = new List<AuditQuery>();
            if (controlDetail == null)
            {
                auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null).ToList();
            }
            else
                auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null || p.RCMDetailRiskControlDetailID == controlDetail.RCMDetailRiskControlDetailID).ToList();
            var result = auditQuery.Select(p => new { auditQueryNo = p.No + ";" + p.AuditQueryID, auditQueryStatus = p.Status }).ToList();
            SelectList auditQuerySelectList = new SelectList(result, "auditQueryNo", "auditQueryStatus", 0);
            return Json(auditQuerySelectList);

        }

        [HttpPost]
        public ActionResult DeleteAuditQuery(int auditQueryId,int controlId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            AuditQuery aq = db.AuditQuery.Where(p => p.AuditQueryID == auditQueryId).FirstOrDefault();
            AuditQuery delquery = new AuditQuery();
            db.AuditQuery.Remove(aq);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", auditQueryId, "Query", aq, delquery, username);

            var controlDetail = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            var auditQuery = new List<AuditQuery>();
            if (controlDetail == null)
            {
                auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null).ToList();
            }
            else
                auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null || p.RCMDetailRiskControlDetailID == controlDetail.RCMDetailRiskControlDetailID).ToList();
            var result = auditQuery.Select(p => new { auditQueryNo = p.No + ";" + p.AuditQueryID, auditQueryStatus = p.Status }).ToList();
            SelectList auditQuerySelectList = new SelectList(result, "auditQueryNo", "auditQueryStatus", 0);
            return Json(auditQuerySelectList);
            
        }

        [HttpPost]
        public bool DeleteAuditQueryNull()
        {

            List<AuditQuery> aq = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null).ToList();
            foreach (var x in aq)
            {
                db.AuditQuery.Remove(x);
            }
            db.SaveChanges();

            return true;
        }

        public ActionResult EditAuditQuery(int id)
        {
            AuditQuery aq = db.AuditQuery.Find(id);
            ViewBag.empName = db.Employees.Find(aq.Dari).Name;
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var noid = aq.No.Split('/')[0];
            var aqnama = aq.No.Split('/')[1];
            var no = aq.No.Split('/')[2];
            var getFiles = filesTransact.getFiles(noid + aqnama + no, out newFilesName, out paths, url, server);

            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;
            return View(aq);
        }

        public ActionResult SetControlReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCMDetailRiskControl rdrc = db.RCMDetailRiskControls.Find(id);
            return View(rdrc);
        }

        public ActionResult AddDetailIssue(int id, int idSp = 0)
        {
            string auditQueryNo = String.Empty;
            string controlIssueNo = String.Empty;
            AuditQuery lastAuditQuery = db.AuditQuery.OrderByDescending(p => p.AuditQueryID).FirstOrDefault();
            if (lastAuditQuery != null)
            {
                int auditNo = Convert.ToInt32(lastAuditQuery.No.Split('/')[2]) + 1;
                auditQueryNo = Convert.ToString(id + "/AuditQuery/" + auditNo);
            }
            else
                auditQueryNo = Convert.ToString(id + "/AuditQuery/" + 1);

            RCMDetailRiskControlIssue lastIssue = db.RCMDetailRiskControlIssue.OrderByDescending(p => p.NoRef).FirstOrDefault();
            if (lastIssue != null)
            {
                int issueNo = Convert.ToInt32(lastIssue.NoRef.Split('/')[2]) + 1;
                controlIssueNo = Convert.ToString(id + "/ControlIssue/" + issueNo);
            }
            else
                controlIssueNo = Convert.ToString(id + "/ControlIssue/" + 1);

            int controlDetailId = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID.Equals(id)).Select(p => p.RCMDetailRiskControlDetailID).FirstOrDefault();
            ViewBag.auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == controlDetailId).ToList();
            ViewBag.controlDetail = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID.Equals(id)).FirstOrDefault();
            RCMDetailRiskControlIssue controlIssue = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(id)).OrderByDescending(p => p.RCMDetailRiskControlIssueID).FirstOrDefault();
            ViewBag.controlIssue = controlIssue;
            if (controlIssue != null)
            {
                ViewBag.findingtypeddl = controlIssue.FindingType;
                controlIssueNo = controlIssue.NoRef;
            }
            ViewBag.auditQueryNo = auditQueryNo;
            ViewBag.controlIssueNo = controlIssueNo;
            ViewBag.idSp = idSp;
            RCMDetailRiskControl rdrc = db.RCMDetailRiskControls.Find(id);
            return View(rdrc);
        }

        public ActionResult SaveControlDetail(int controlId, string workDoneDesc, string workDoneRes)
        {
            string username = User.Identity.Name;
            RCMDetailRiskControlDetail rdrcd = new RCMDetailRiskControlDetail();
            rdrcd.RCMDetailRiskControlID = controlId;
            rdrcd.WorkDoneDescription = workDoneDesc;
            rdrcd.WorkDoneResult = workDoneRes;
            db.RCMDetailRiskControlDetail.Add(rdrcd);
            db.SaveChanges();
            RCMDetailRiskControlDetail condetail = new RCMDetailRiskControlDetail();
            auditTransact.CreateAuditTrail("Create", rdrcd.RCMDetailRiskControlDetailID, "Control Detail", condetail, rdrcd, username);
            List<AuditQuery> aqs = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null).ToList();
            foreach (var aq in aqs)
            {
                aq.RCMDetailRiskControlDetailID = rdrcd.RCMDetailRiskControlDetailID;
                db.Entry(aq).State = EntityState.Modified;
            }
            db.SaveChanges();
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        public ActionResult UpdateControlDetail(int controlId, string workDoneDesc, string workDoneRes)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControlDetail rdrcd = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();
            rdrcd.RCMDetailRiskControlID = controlId;
            rdrcd.WorkDoneDescription = workDoneDesc;
            rdrcd.WorkDoneResult = workDoneRes;
            RCMDetailRiskControlDetail oldData = db.RCMDetailRiskControlDetail.AsNoTracking().Where(p => p.RCMDetailRiskControlDetailID.Equals(rdrcd.RCMDetailRiskControlDetailID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", rdrcd.RCMDetailRiskControlDetailID, "Control Detail", oldData, rdrcd, username);
            db.Entry(rdrcd).State = EntityState.Modified;
            db.SaveChanges();
            List<AuditQuery> aqs = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null).ToList();
            foreach (var aq in aqs)
            {
                aq.RCMDetailRiskControlDetailID = rdrcd.RCMDetailRiskControlDetailID;
                db.Entry(aq).State = EntityState.Modified;
            }
            db.SaveChanges();
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [ValidateInput(false)]
        public ActionResult SaveControlIssue(string status, int controlId, string noref, string title, string fact, string cri, string imp, string impactval, string coz, string rec, string managres, string findtype, string picName)
        {
            bool iscreate = false;
            RCMDetailRiskControlIssue rdrci = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();

            //RCMDetailRiskControlIssue rdrci = new RCMDetailRiskControlIssue();
            if (rdrci == null)
            {
                iscreate = true;
                rdrci = new RCMDetailRiskControlIssue();
            }

            string username = User.Identity.Name;
            
            rdrci.RCMDetailRiskControlID = controlId;
            rdrci.NoRef = noref;
            rdrci.Title = title;
            rdrci.Fact = fact;
            rdrci.Criteria = cri;
            rdrci.Impact = imp;
            rdrci.ImpactValue = impactval;
            rdrci.Cause = coz;
            rdrci.Recommendation = rec;
            rdrci.ManagementResponse = managres;
            rdrci.FindingType = findtype;
            rdrci.PICID = picName;
            if (rdrci.Status_App != "Draft" && status == "Draft")
                rdrci.Status_App = HelperController.GetStatusSendback(db, "Control Issue", rdrci.Status_App);
            else
                rdrci.Status_App = status;
            //db.RCMDetailRiskControlIssue.Add(rdrci);
            if (iscreate)
            {
                db.RCMDetailRiskControlIssue.Add(rdrci);
            }
            else
            {
                //RCMDetailRiskControlIssue oldData = db.RCMDetailRiskControlIssue.AsNoTracking().Where(p => p.RCMDetailRiskControlIssueID.Equals(rdrci.RCMDetailRiskControlIssueID)).FirstOrDefault();
                //auditTransact.CreateAuditTrail("Update", rdrci.RCMDetailRiskControlIssueID, "Control Issue", oldData, rdrci, username);
                db.Entry(rdrci).State = EntityState.Modified;
            }
            if (iscreate)
            {
                RCMDetailRiskControlIssue createissue = new RCMDetailRiskControlIssue();
                auditTransact.CreateAuditTrail("Create", rdrci.RCMDetailRiskControlIssueID, "Control Issue", createissue, rdrci, username);
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "issue";
                rrm.Description = page + rdrci.RCMDetailRiskControlID;
                db.ReviewRelationMasters.Add(rrm);
            }
            db.SaveChanges();
            //RCMDetailRiskControlIssue createissue = new RCMDetailRiskControlIssue();
            //auditTransact.CreateAuditTrail("Create", rdrci.RCMDetailRiskControlIssueID, "Control Issue", createissue, rdrci, username);
            //ReviewRelationMaster rrm = new ReviewRelationMaster();
            //string page = "issue";
            //rrm.Description = page + rdrci.RCMDetailRiskControlID;
            //db.ReviewRelationMasters.Add(rrm);
            //db.SaveChanges();
            
            
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [ValidateInput(false)]
        public ActionResult UpdateControlIssue(int controlId, string noref, string title, string fact, string cri, string imp, string impactval, string coz, string rec, string managres, string findtype, string picName, string status)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControlIssue rdrci = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();
            rdrci.RCMDetailRiskControlID = controlId;
            rdrci.NoRef = noref;
            rdrci.Title = title;
            rdrci.Fact = fact;
            rdrci.Criteria = cri;
            rdrci.Impact = imp;
            rdrci.ImpactValue = impactval;
            rdrci.Cause = coz;
            rdrci.Recommendation = rec;
            rdrci.ManagementResponse = managres;
            rdrci.FindingType = findtype;
            rdrci.PICID = picName;
            if (rdrci.Status_App != "Draft" && status == "Draft")
                rdrci.Status_App = HelperController.GetStatusSendback(db, "Control Issue", rdrci.Status_App);
            else
                rdrci.Status_App = status;
            RCMDetailRiskControlIssue oldData = db.RCMDetailRiskControlIssue.AsNoTracking().Where(p => p.RCMDetailRiskControlIssueID.Equals(rdrci.RCMDetailRiskControlIssueID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", rdrci.RCMDetailRiskControlIssueID, "Control Issue", oldData, rdrci, username);
            db.Entry(rdrci).State = EntityState.Modified;
            db.SaveChanges();
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [ValidateInput(false)]
        public ActionResult SubmitAuditQuery(string no, string tanggal, string kepada,string status, string dari, string lampiran, string perihal, string pembuka, string issuedesc, string criteria, string impact, string auditeeclar, string penutup, string folder, int controlId)
        {
            string username = User.Identity.Name;
            AuditQuery aq = new AuditQuery();
            //int kepadaId = db.Employees.Where(p => p.Name == kepada).Select(p => p.EmployeeID).FirstOrDefault();
            int dariId = db.Employees.Where(p => p.Name == dari).Select(p => p.EmployeeID).FirstOrDefault();
            aq.No = no; aq.Tanggal = (tanggal != String.Empty) ? Convert.ToDateTime(tanggal) : DateTime.Now; aq.Kepada = kepada; aq.Dari = dariId; aq.Lampiran = lampiran;
            aq.Perihal = perihal; aq.Pembuka = pembuka; aq.IssueDesc = issuedesc; aq.Criteria = criteria; aq.Impact = impact;
            aq.AuditeeClarification = auditeeclar; aq.Penutup = penutup; aq.Status = status; aq.Folder = folder;
            db.AuditQuery.Add(aq);
            db.SaveChanges();
            AuditQuery createquery = new AuditQuery();
            auditTransact.CreateAuditTrail("Create", aq.AuditQueryID, "Audit Query", createquery, aq, username);

            ReviewRelationMaster rrm = new ReviewRelationMaster();
            string page = "AQ";
            rrm.Description = page + aq.AuditQueryID;
            db.ReviewRelationMasters.Add(rrm);
            db.SaveChanges();

            var controlDetail = db.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID == controlId).FirstOrDefault();
            var auditQuery = new List<AuditQuery>();
            if (controlDetail == null)
            {
                auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null).ToList();
            }
            else
            {
                aq.RCMDetailRiskControlDetailID = controlDetail.RCMDetailRiskControlDetailID;
                AuditQuery audit = db.AuditQuery.Find(aq.AuditQueryID);
                db.Entry(audit).State = EntityState.Modified;
                db.SaveChanges();
                auditQuery = db.AuditQuery.Where(p => p.RCMDetailRiskControlDetailID == null || p.RCMDetailRiskControlDetailID == controlDetail.RCMDetailRiskControlDetailID).ToList();
            } 
            var result = auditQuery.Select(p => new { auditQueryNo = p.No + ";" + p.AuditQueryID, auditQueryStatus = p.Status }).ToList();
            SelectList auditQuerySelectList = new SelectList(result, "auditQueryNo", "auditQueryStatus", 0);
            return Json(auditQuerySelectList);
        }

        public JsonResult UploadFile()
        {
            AuditQuery auditq = db.AuditQuery.OrderByDescending(p => p.AuditQueryID).FirstOrDefault();
            HttpServerUtilityBase server = Server;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i];
                if (file != null)
                {
                    i = i + 1;
                    var id = auditq.No.Split('/')[0];
                    var aq = auditq.No.Split('/')[1];
                    var no = auditq.No.Split('/')[2];
                    bool addFile = filesTransact.addFile(id + aq + no, i, file, server);
                }
            }
            return Json("Uploaded " + Request.Files.Count + " files");
        }

        public ActionResult AuditQueryAutocomplete(string term)
        {
            var items = db.Employees.Select(p => p.Name).ToList();

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

        #endregion

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
        }

        private bool checkCanApprove(int fwId)
        {
            bool result = true;
            FieldWork fw = db.FieldWorks.AsNoTracking().Where(p => p.FieldWorkID == fwId).FirstOrDefault();
            List<int> riskIds = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == fw.RiskControlMatrixID).Select(p => p.RCMDetailRiskID).ToList();
            if (riskIds.Count() > 0)
            {
                List<int> controlIds = db.RCMDetailRiskControls.Where(p => (riskIds.Contains(p.RCMDetailRiskID))).Select(p => p.RCMDetailRiskControlID).ToList();
                if (controlIds.Count() > 0)
                {
                    List<string> detailIds = db.RCMDetailRiskControlDetail.Where(p => (controlIds.Contains(p.RCMDetailRiskControlID))).Select(p => p.RCMDetailRiskControlDetailID.ToString()).ToList();
                    if (detailIds.Count() > 0)
                    {
                        List<string> aqStatus = db.AuditQuery.Where(p => (detailIds.Contains(p.RCMDetailRiskControlDetailID.ToString()))).Select(p => p.Status).ToList();
                        if (aqStatus.Count() > 0)
                        {
                            bool haveAqPending = aqStatus.Any(p => p.StartsWith("Pending"));
                            if (aqStatus.Contains("Draft") || haveAqPending)
                                result = false;
                        }
                    }
                    List<string> issueStatus = new List<string>();
                    foreach (int item in controlIds)
	                {
                        issueStatus.Add(db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID == item ).OrderByDescending(d => d.RCMDetailRiskControlIssueID).Select(p => p.Status_App).FirstOrDefault());
	                }
                    issueStatus = issueStatus.Where(d => d != null).ToList();
                    //List<string> issueStatus = db.RCMDetailRiskControlIssue.Where(p => (controlIds.Contains(p.RCMDetailRiskControlID))).Select(p => p.Status_App).ToList();
                    if (issueStatus.Count() > 0)
                    {
                        bool haveIssuePending = issueStatus.Any(p => p.StartsWith("Pending"));
                        if (issueStatus.Contains("Draft") || haveIssuePending)
                            result = false;
                    }
                }
            }
            return result;
        }

        [HttpPost]
        public JsonResult GetRiskControl(int Id)
        {
            int idPre = db.LetterOfCommands.Where(d => d.LetterOfCommandID == Id).Select(d => d.AuditPlanningMemorandum.Preliminary.PreliminaryID).FirstOrDefault();
            //var walk = new SelectList(db.RiskControlMatrixs.Where(p => p.Status.Equals("Approve") && p.BusinessProces.Walktrough.PreliminaryID == idPre), "RiskControlMatrixID", "Objectives");
            //var walk = db.Walktroughs.Where(d => d.PreliminaryID == idPre).Select(d => d.RCM);
            int idwalk = db.Walktroughs.Where(d => d.PreliminaryID == idPre).Select(d=>d.WalktroughID).FirstOrDefault();
            var RiskControl = new SelectList(db.RiskControlMatrixs.Where(d => d.WalktroughID == idwalk && d.Status == "Approve"), "RiskControlMatrixID", "Objectives");
            return Json(RiskControl, JsonRequestBehavior.AllowGet);
        }
    }
}

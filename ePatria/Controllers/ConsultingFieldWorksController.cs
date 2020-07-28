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
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Controllers
{
    [Authorize()]
    public class ConsultingFieldWorksController : Controller
    {
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private FilesUploadController filesTransact = new FilesUploadController();
        private HelperController helperTransact = new HelperController();

        // GET: ConsultingFieldWorks
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
            var consultingFieldWork = db.ConsultingFieldWork.Include(c => c.ConsultingLetterOfCommand);
            return View(consultingFieldWork.ToList());
        }

        // GET: ConsultingFieldWorks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingFieldWork consultingFieldWork = db.ConsultingFieldWork.Find(id);
            if (consultingFieldWork == null)
            {
                return HttpNotFound();
            }
            return View(consultingFieldWork);
        }

        // GET: ConsultingFieldWorks/Create
        public ActionResult Create()
        {
            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP");
            return View();
        }

        // POST: ConsultingFieldWorks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConsultingFieldWorkID,ConsultingSuratPerintahID,Desc")] ConsultingFieldWork consultingFieldWork)
        {
            if (ModelState.IsValid)
            {
                db.ConsultingFieldWork.Add(consultingFieldWork);
                db.SaveChanges();
                TempData["message"] = "Consulting Field Work successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingFieldWork.ConsultingSuratPerintahID);
            return View(consultingFieldWork);
        }

        // GET: ConsultingFieldWorks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingFieldWork fieldWork = db.ConsultingFieldWork.Find(id);
            if (fieldWork == null)
            {
                return HttpNotFound();
            }

            ViewBag.LetterOfCommandID = new SelectList(db.ConsultingLetterOfCommands, "LetterOfCommandID", "NomorSP", fieldWork.ConsultingSuratPerintahID);
            ViewBag.RiskControlMatrixID = new SelectList(db.ConsultingRiskControlMatrixs, "RiskControlMatrixID", "BusinessProcesID", fieldWork.ConsultingRCMID);

            ConsultingRiskControlMatrix rcm = db.ConsultingRiskControlMatrixs.Find(fieldWork.ConsultingRCMID);
            if (rcm != null)
            {
                ViewBag.ConsultingBPM = db.ConsultingBusinessProcess.Where(p => p.ConsultingBPMID.Equals(rcm.ConsultingBPMID)).Select(p => p.DocumentName).FirstOrDefault();
                ViewBag.ConsultingSubBP = rcm.SubBusinessProcess;
                ViewBag.ConsultingObj = rcm.Objectives;
                ViewBag.RiskControlMatrixID = fieldWork.ConsultingRCMID;
                ViewBag.LetterOfCommandID = fieldWork.ConsultingSuratPerintahID;
                var riskList = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID == rcm.ConsultingRCMID).Select(p => p.ConsultingRCMDetailRiskID).ToList();
                var allControl = db.ConsultingRCMDetailRiskControls.Where(p => riskList.Contains(p.ConsultingRCMDetailRiskID)).ToList();
                var allNotPendingControl = allControl.Where(p => !p.Status.Equals("Pending")).ToList();
                double controlCount = ((double)allNotPendingControl.Count() / (double)allControl.Count()) * 100;
                ViewBag.ControlCount = controlCount;
                var allReviewedControl = allControl.Where(p => !String.IsNullOrEmpty(p.ReviewStatus)).ToList();
                double reviewedCount = ((double)allReviewedControl.Count() / (double)allControl.Count()) * 100;
                ViewBag.reviewedCount = reviewedCount;
                var rdr = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID == rcm.ConsultingRCMID).Select(p => new { riskId = p.ConsultingRCMDetailRiskID, riskName = p.RiskName }).ToList();
                ViewBag.RDR = new SelectList(rdr, "riskId", "riskName");
            }
            else {
                ViewBag.ControlCount = 0;
                ViewBag.reviewedCount = 0;
                ViewBag.RDR = new SelectList(new List<ConsultingRCMDetailRisk>(), "riskId", "riskName");
            }


            ConsultingRiskControlMatrix lastRCM = db.ConsultingRiskControlMatrixs.OrderByDescending(p => p.ConsultingRCMID).FirstOrDefault();
            int newRCMId = 1;
            if (lastRCM != null)
                newRCMId = lastRCM.ConsultingRCMID + 1;
            ViewBag.newRCMId = newRCMId;



            List<ConsultingFieldWork> fieldWorks = db.ConsultingFieldWork.Where(p => p.ConsultingFieldWorkID.Equals(fieldWork.ConsultingFieldWorkID)).ToList();
            List<ConsultingRCMDetailRisk> risks = new List<ConsultingRCMDetailRisk>();
            List<ConsultingRCMDetailRiskControl> controls = new List<ConsultingRCMDetailRiskControl>();
            List<ConsultingRCMDetailRiskControlIssue> issueList = new List<ConsultingRCMDetailRiskControlIssue>();
            List<ConsultingAuditQuery> auditQueryList = new List<ConsultingAuditQuery>();
            ConsultingRCMDetailRiskControlIssue issue = new ConsultingRCMDetailRiskControlIssue();
            foreach (var fieldwork in fieldWorks)
            {
                if (fieldwork.ConsultingRCMID != null)
                {
                    risks = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRiskControlMatrix.ConsultingRCMID.Equals(fieldwork.ConsultingRiskControlMatrix.ConsultingRCMID)).ToList();
                    foreach (var risk in risks)
                    {
                        controls = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID.Equals(risk.ConsultingRCMDetailRiskID)).ToList();
                        //List<ConsultingRCMDetailRiskControlIssue> Issues = new List<ConsultingRCMDetailRiskControlIssue>();
                        foreach (var control in controls)
                        {
                            List<ConsultingRCMDetailRiskControlIssue> Issues = db.ConsultingRCMDetailRiskControlIssue.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(control.ConsultingRCMDetailRiskControlID)).ToList();
                            foreach (var iss in Issues)
                            {

                                issueList.Add(iss);

                            }

                            List<ConsultingAuditQuery> AuditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetail.ConsultingRCMDetailRiskControlID.Equals(control.ConsultingRCMDetailRiskControlID)).ToList();
                            foreach (var item in AuditQuery)
                            {
                                //iss.Fact = System.Text.RegularExpressions.Regex.Replace(iss.Fact, @"<[^>]*>", String.Empty);
                                auditQueryList.Add(item);
                            }
                        }
                    }
                }
            }

            ViewBag.Issues = issueList;
            ViewBag.AuditQuery = auditQueryList;

            return View(fieldWork);
        }

        private bool sentEmailCFW(ConsultingFieldWork consultingFieldWork, string user)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
            List<Employee> CIAEmpList = new List<Employee>();
            var no = db.ConsultingLetterOfCommands.Where(p => p.ConsultingSuratPerintahID.Equals(consultingFieldWork.ConsultingSuratPerintahID)).Select(p => p.NomorSP).FirstOrDefault();
            if (CIAUserIds.Count() > 0)
            {
                var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                foreach (var CIAUser in CIAUsers)
                {
                    Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();
                    if (emp != null)
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>Consulting Fieldwork for SP : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Consulting Fieldwork\">link</a> to show the Consulting Fieldwork.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/ConsultingFieldWorks/Edit/" + consultingFieldWork.ConsultingFieldWorkID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, no, emailContent, url);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailCFW(string userToSentEmail, ConsultingFieldWork consultingFieldWork)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            var no = db.ConsultingLetterOfCommands.Where(p => p.ConsultingSuratPerintahID.Equals(consultingFieldWork.ConsultingSuratPerintahID)).Select(p => p.NomorSP).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>Consulting Fieldwork for SP : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Consulting Fieldwork\">link</a> to show the Consulting Fieldwork.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = baseUrl + "/ConsultingFieldWorks/Edit/" + consultingFieldWork.ConsultingFieldWorkID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, no, emailContent, url);
            }
            return true;
        }

        // POST: ConsultingFieldWorks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConsultingFieldWorkID,ConsultingSuratPerintahID,Desc,ConsultingRCMID,Status")] ConsultingFieldWork consultingFieldWork, string submit)
        {
            if (ModelState.IsValid)
            {
                ConsultingLetterOfCommand csp = db.ConsultingLetterOfCommands.Find(consultingFieldWork.ConsultingSuratPerintahID);
				string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    consultingFieldWork.Status = "Draft";
                else if (submit == "Send Back")
                    consultingFieldWork.Status = HelperController.GetStatusSendback(db, "Consulting Fieldwork ", consultingFieldWork.Status);
                else if (submit == "Approve")
                {
                    if (checkCanApprove(consultingFieldWork.ConsultingFieldWorkID))
                    {
                        consultingFieldWork.Status = "Approve";
                        ConsultingReporting newReporting = new ConsultingReporting();
                        newReporting.ConsultingSuratPerintahID = consultingFieldWork.ConsultingSuratPerintahID;
                        newReporting.ConsultingFieldWorkID = consultingFieldWork.ConsultingFieldWorkID;
                        newReporting.NoLaporan = helperTransact.getNewNumber("CReporting");
                        db.ConsultingReportings.Add(newReporting);
                        ConsultingFeedback newFeedback = new ConsultingFeedback();
                        newFeedback.ConsultingSuratPerintahID = consultingFieldWork.ConsultingSuratPerintahID;
                        newFeedback.ConsultingFieldWorkID = consultingFieldWork.ConsultingFieldWorkID;
                        db.ConsultingFeedbacks.Add(newFeedback);
                    }
                    else
                    {
                        TempData["messageerror"] = "Cannot Approve This FieldWork Because Still Have Audit Queries or Issues That Not Yet Approved";
                        return RedirectToAction("Index");
                    }
                }
                else if (submit == "Submit For Review By" + user)
                {
                    if (checkCanApprove(consultingFieldWork.ConsultingFieldWorkID))
                    {
                        consultingFieldWork.Status = "Pending for Review by" + user;
                    }
                    else
                    {
                        TempData["messageerror"] = "Cannot Submit This FieldWork Because Still Have Audit Queries or Issues That Not Yet Approved";
                        return RedirectToAction("Index");
                    }
                }
                else if (submit == "Submit For Approve By" + user)
                {
                    if (checkCanApprove(consultingFieldWork.ConsultingFieldWorkID))
                    {
                        consultingFieldWork.Status = "Pending for Approve by" + user;
                    }
                    else
                    {
                        TempData["messageerror"] = "Cannot Submit This FieldWork Because Still Have Audit Queries or Issues That Not Yet Approved";
                        return RedirectToAction("Index");
                    }
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = csp.PicID;
                        if (userToSentEmail != null)
                            sentSingleEmailCFW(userToSentEmail, consultingFieldWork);
                        else
                            sentEmailCFW(consultingFieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = csp.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailCFW(userToSentEmail, consultingFieldWork);
                        else
                            sentEmailCFW(consultingFieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = csp.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailCFW(userToSentEmail, consultingFieldWork);
                        else
                            sentEmailCFW(consultingFieldWork, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = csp.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailCFW(userToSentEmail, consultingFieldWork);
                        else
                            sentEmailCFW(consultingFieldWork, user.Trim());
                    }
                }
                ConsultingFieldWork oldData = db.ConsultingFieldWork.AsNoTracking().Where(p => p.ConsultingFieldWorkID.Equals(consultingFieldWork.ConsultingFieldWorkID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", consultingFieldWork.ConsultingFieldWorkID, "Consulting Field Work", oldData, consultingFieldWork, username);
                db.Entry(consultingFieldWork).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Consulting Field Work successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.LetterOfCommandID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingFieldWork.ConsultingSuratPerintahID);
            ViewBag.RiskControlMatrixID = new SelectList(db.ConsultingRiskControlMatrixs, "ConsultingRCMID", "Objectives", consultingFieldWork.ConsultingRCMID);
            return View(consultingFieldWork);
        }

        public ActionResult EditStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingRCMDetailRiskControlIssue issue = db.ConsultingRCMDetailRiskControlIssue.Find(id);
            ViewBag.Status_Approval = issue.Status_Approval;
            return View(issue);
        }

        [ValidateInput(false)]
        public bool UpdateStatusIssue(int issueID, string status, int id)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailRiskControlIssue issue = db.ConsultingRCMDetailRiskControlIssue.Find(issueID);
            issue.Status_Approval = status;
            ConsultingRCMDetailRiskControlIssue oldData = db.ConsultingRCMDetailRiskControlIssue.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskControlIssueID.Equals(issue.ConsultingRCMDetailRiskControlIssueID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", issue.ConsultingRCMDetailRiskControlIssueID, "Consulting Status Issue", oldData, issue, username);
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
                            string url = baseUrl + "/ConsultingFieldWorks/Edit/" + id;
                            emailTransact.SentEmailApproval(emp.Email, emp.Name, issue.Title, emailContent, url);

                        }
                    }
                }
            }
            return true;
        }

        // GET: ConsultingFieldWorks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingFieldWork consultingFieldWork = db.ConsultingFieldWork.Find(id);
            if (consultingFieldWork == null)
            {
                return HttpNotFound();
            }
            return View(consultingFieldWork);
        }

        // POST: ConsultingFieldWorks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingFieldWork consultingFieldWork = db.ConsultingFieldWork.Find(id);
            ConsultingFieldWork confieldwork = new ConsultingFieldWork();
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Field Work", consultingFieldWork, confieldwork, username);
            db.ConsultingFieldWork.Remove(consultingFieldWork);
            db.SaveChanges();
            TempData["message"] = "Consulting Field Work successfully deleted!";
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

        [HttpPost]
        public ActionResult SaveRisk(int rcmId, string riskName, string bpm, string subbp, string obj, int fwid)
        {
            ConsultingBusinessProcess bp = db.ConsultingBusinessProcess.Where(p => p.DocumentName.Equals(bpm)).FirstOrDefault();
            ConsultingRiskControlMatrix cRCM = new ConsultingRiskControlMatrix();
            if (bp == null)
            {
                ConsultingBusinessProcess newBP = new ConsultingBusinessProcess();
                newBP.DocumentName = bpm;
                db.ConsultingBusinessProcess.Add(newBP);
                db.SaveChanges();
                cRCM.ConsultingBPMID = newBP.ConsultingBPMID;
                cRCM.SubBusinessProcess = subbp;
                cRCM.Objectives = obj;
                db.ConsultingRiskControlMatrixs.Add(cRCM);
                db.SaveChanges();
            }

            ConsultingFieldWork cF = db.ConsultingFieldWork.Find(fwid);
            if (cF.ConsultingRCMID == null)
            {
                cF.ConsultingRCMID = cRCM.ConsultingRCMID != null ? cRCM.ConsultingRCMID : rcmId;
                db.Entry(cF).State = EntityState.Modified;
            }

            string username1 = User.Identity.Name;
            ConsultingRCMDetailRisk newRdr = new ConsultingRCMDetailRisk();
            newRdr.ConsultingRCMID = (Int32)cF.ConsultingRCMID;
            newRdr.RiskName = riskName;
            newRdr.Status = "Pending";
            db.ConsultingRCMDetailRisks.Add(newRdr);
            db.SaveChanges();
            ConsultingRCMDetailRisk createrisk = new ConsultingRCMDetailRisk();
            //auditTransact.CreateAuditTrail("Create", newRdr.ConsultingRCMDetailRiskID, "Consulting Risk", createrisk, newRdr, username1);
            var rdr = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID == cF.ConsultingRCMID).Select(p => new { riskId = p.ConsultingRCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName");
            var result = new { Result = riskSelectList, rcmId = cF.ConsultingRCMID };
            return Json(result);
        }

        [HttpPost]
        public ActionResult GetControlsByRiskId(int riskId)
        {
            var riskControl = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID == riskId).Select(p => new { controlId = p.ConsultingRCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        public int GetCountStatusByRiskId(int riskId)
        {
            var controlList = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID.Equals(riskId) && !p.Status.Equals("Pending")).ToList();
            return controlList.Count();
        }

        public int GetCountReviewByRiskId(int riskId)
        {
            var controlList = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID.Equals(riskId) && !String.IsNullOrEmpty(p.ReviewStatus)).ToList();
            return controlList.Count();
        }

        public string GetAllStatusAndReviewControl(int rcmId)
        {
            var riskList = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID.Equals(rcmId)).Select(p => p.ConsultingRCMDetailRiskID).ToList();
            var allControl = db.ConsultingRCMDetailRiskControls.Where(p => riskList.Contains(p.ConsultingRCMDetailRiskID)).ToList();
            var allNotPendingControl = allControl.Where(p => !p.Status.Equals("Pending")).ToList();
            double controlCount = ((double)allNotPendingControl.Count() / (double)allControl.Count()) * 100;
            var allReviewedControl = allControl.Where(p => !String.IsNullOrEmpty(p.ReviewStatus)).ToList();
            double reviewedCount = ((double)allReviewedControl.Count() / (double)allControl.Count()) * 100;
            string result = Convert.ToString(controlCount) + ";" + Convert.ToString(reviewedCount);
            return result;
        }



        [HttpPost]
        public ActionResult UpdateRisk(int rcmId, int riskId, string riskName)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailRisk Rdr = db.ConsultingRCMDetailRisks.Find(riskId);
            Rdr.RiskName = riskName;
            ConsultingRCMDetailRisk oldData = db.ConsultingRCMDetailRisks.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskID.Equals(Rdr.ConsultingRCMDetailRiskID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", Rdr.ConsultingRCMDetailRiskID, "Consulting Risk", oldData, Rdr, username);
            db.Entry(Rdr).State = EntityState.Modified;
            db.SaveChanges();
            var rdr = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID == rcmId).Select(p => new { riskId = p.ConsultingRCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult DeleteRisk(int rcmId, int riskId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingRCMDetailRisk Rdr = db.ConsultingRCMDetailRisks.Find(riskId);
            ConsultingRCMDetailRisk deleterisk = new ConsultingRCMDetailRisk();
            db.ConsultingRCMDetailRisks.Remove(Rdr);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", riskId, "Consulting Risk", Rdr, deleterisk, username);
            var rdr = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID == rcmId).Select(p => new { riskId = p.ConsultingRCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult SaveControl(int riskId, string controlName)
        {
            string username = User.Identity.Name;
            ConsultingRCMDetailRiskControl newControl = new ConsultingRCMDetailRiskControl();
            newControl.ConsultingRCMDetailRiskID = riskId;
            newControl.ControlName = controlName;
            newControl.DueDate = DateTime.Now;
            newControl.CloseDate = DateTime.Now;
            newControl.Status = "Pending";
            db.ConsultingRCMDetailRiskControls.Add(newControl);
            db.SaveChanges();
            ConsultingRCMDetailRiskControl createcontrol = new ConsultingRCMDetailRiskControl();
            auditTransact.CreateAuditTrail("Create", newControl.ConsultingRCMDetailRiskControlID, "Consulting Control", createcontrol, newControl, username);
            ReviewRelationMaster rrm = new ReviewRelationMaster();
            string page = "ccont";
            rrm.Description = page + newControl.ConsultingRCMDetailRiskControlID;
            db.ReviewRelationMasters.Add(rrm);
            db.SaveChanges();
            var riskControl = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID == riskId).Select(p => new { controlId = p.ConsultingRCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult GetAuditStepByRiskControlId(int controlId)
        {
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult UpdateControl(int riskId, int controlId, string controlName)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailRiskControl control = db.ConsultingRCMDetailRiskControls.Find(controlId);
            control.ControlName = controlName;
            ConsultingRCMDetailRiskControl oldData = db.ConsultingRCMDetailRiskControls.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskControlID.Equals(control.ConsultingRCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", control.ConsultingRCMDetailRiskControlID, "Consulting Control", oldData, control, username);
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            var riskControl = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID == riskId).Select(p => new { controlId = p.ConsultingRCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult SetControlStatus(int riskId, int controlId, string controlStatus)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailRiskControl control = db.ConsultingRCMDetailRiskControls.Find(controlId);
            control.Status = controlStatus;
            ConsultingRCMDetailRiskControl oldData = db.ConsultingRCMDetailRiskControls.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskControlID.Equals(control.ConsultingRCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", control.ConsultingRCMDetailRiskControlID, "Consulting Control Status", oldData, control, username);
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            var riskControl = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID == riskId).Select(p => new { controlId = p.ConsultingRCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult DeleteControl(int riskId, int controlId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingRCMDetailRiskControlDetail cdetail = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault();
            ConsultingRCMDetailRiskControlIssue cIssue = db.ConsultingRCMDetailRiskControlIssue.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault();
            if (cdetail == null && cIssue == null)
            {
                ConsultingRCMDetailRiskControl control = db.ConsultingRCMDetailRiskControls.Find(controlId);
                ConsultingRCMDetailRiskControl deletecontrol = new ConsultingRCMDetailRiskControl();
                db.ConsultingRCMDetailRiskControls.Remove(control);
                db.SaveChanges();
                auditTransact.CreateAuditTrail("Delete", controlId, "Consulting Control", control, deletecontrol, username);
            }
            var riskControl = db.ConsultingRCMDetailRiskControls.Where(p => p.ConsultingRCMDetailRiskID == riskId).Select(p => new { controlId = p.ConsultingRCMDetailRiskControlID, controlName = p.ControlName + " - " + p.Status + " - " + p.ReviewStatus }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult SaveAudit(int controlId, string auditName)
        {
            string username = User.Identity.Name;
            ConsultingRCMDetailControlAuditStep newAudit = new ConsultingRCMDetailControlAuditStep();
            newAudit.ConsultingRCMDetailRiskControlID = controlId;
            newAudit.AuditStepName = auditName;
            newAudit.Status = "Pending";
            db.ConsultingRCMDetailControlAuditSteps.Add(newAudit);
            db.SaveChanges();
            ConsultingRCMDetailControlAuditStep createaudit = new ConsultingRCMDetailControlAuditStep();
            auditTransact.CreateAuditTrail("Create", newAudit.ConsultingRCMDetailControlAuditStepID, "Consulting Audit", createaudit, newAudit, username);
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult UpdateAudit(int controlId, string auditName)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailControlAuditStep audit = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault(); audit.AuditStepName = auditName;
            ConsultingRCMDetailControlAuditStep oldData = db.ConsultingRCMDetailControlAuditSteps.AsNoTracking().Where(p => p.ConsultingRCMDetailControlAuditStepID.Equals(audit.ConsultingRCMDetailControlAuditStepID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", audit.ConsultingRCMDetailControlAuditStepID, "Consulting Audit", oldData, audit, username);
            db.Entry(audit).State = EntityState.Modified;
            db.SaveChanges();
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult DeleteAudit(int controlId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingRCMDetailControlAuditStep deleteaudit = new ConsultingRCMDetailControlAuditStep();
            ConsultingRCMDetailControlAuditStep audit = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault(); db.ConsultingRCMDetailControlAuditSteps.Remove(audit);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", controlId, "Consulting Audit", audit, deleteaudit, username);
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateAuditQuery(string no, string tanggal, string kepada, string dari, string lampiran, string perihal, string pembuka, string issuedesc, string criteria, string impact, string auditeeclar, string penutup, string folder, int controlId, string status)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingAuditQuery aq = db.ConsultingAuditQuery.Where(p => p.No.Equals(no)).FirstOrDefault();

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
            aq.AuditeeClarification = auditeeclar; aq.Penutup = penutup; aq.Status = status; aq.Folder = folder;
            ConsultingAuditQuery oldData = db.ConsultingAuditQuery.AsNoTracking().Where(p => p.ConsultingAuditQueryID.Equals(aq.ConsultingAuditQueryID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", aq.ConsultingAuditQueryID, "Consulting Audit Query", oldData, aq, username);
            db.Entry(aq).State = EntityState.Modified;
            db.SaveChanges();

            var controlDetail = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault();
            var auditQuery = new List<ConsultingAuditQuery>();
            if (controlDetail == null)
            {
                auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null).ToList();
            }
            else
                auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null || p.ConsultingRCMDetailRiskControlDetailID == controlDetail.ConsultingRCMDetailRiskControlDetailID).ToList();
            var result = auditQuery.Select(p => new { auditQueryNo = p.No + ";" + p.ConsultingAuditQueryID, auditQueryStatus = p.Status }).ToList();
            SelectList auditQuerySelectList = new SelectList(result, "auditQueryNo", "auditQueryStatus", 0);
            return Json(auditQuerySelectList);

        }

        [HttpPost]
        public ActionResult DeleteAuditQuery(int auditQueryId, int controlId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingAuditQuery aq = db.ConsultingAuditQuery.Where(p => p.ConsultingAuditQueryID == auditQueryId).FirstOrDefault();
            ConsultingAuditQuery delquery = new ConsultingAuditQuery();
            db.ConsultingAuditQuery.Remove(aq);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", auditQueryId, "Consulting Query", aq, delquery, username);

            var controlDetail = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault();
            var auditQuery = new List<ConsultingAuditQuery>();
            if (controlDetail == null)
            {
                auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null).ToList();
            }
            else
                auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null || p.ConsultingRCMDetailRiskControlDetailID == controlDetail.ConsultingRCMDetailRiskControlDetailID).ToList();
            var result = auditQuery.Select(p => new { auditQueryNo = p.No + ";" + p.ConsultingAuditQueryID, auditQueryStatus = p.Status }).ToList();
            SelectList auditQuerySelectList = new SelectList(result, "auditQueryNo", "auditQueryStatus", 0);
            return Json(auditQuerySelectList);

        }

        [HttpPost]
        public bool DeleteAuditQueryNull()
        {

            List<ConsultingAuditQuery> aq = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null).ToList();
            foreach (var x in aq)
            {
                db.ConsultingAuditQuery.Remove(x);
            }
            db.SaveChanges();

            return true;
        }

        public ActionResult EditAuditQuery(int id)
        {
            ConsultingAuditQuery aq = db.ConsultingAuditQuery.Find(id);
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
            ConsultingRCMDetailRiskControl rdrc = db.ConsultingRCMDetailRiskControls.Find(id);
            return View(rdrc);
        }

        public ActionResult AddDetailIssue(int id)
        {
            string auditQueryNo = String.Empty;
            string controlIssueNo = String.Empty;
            ConsultingAuditQuery lastAuditQuery = db.ConsultingAuditQuery.OrderByDescending(p => p.ConsultingAuditQueryID).FirstOrDefault();
            if (lastAuditQuery != null)
            {
                int auditNo = Convert.ToInt32(lastAuditQuery.No.Split('/')[2]) + 1;
                auditQueryNo = Convert.ToString(id + "/ConsultingAuditQuery/" + auditNo);
            }
            else
                auditQueryNo = Convert.ToString(id + "/ConsultingAuditQuery/" + 1);

            ConsultingRCMDetailRiskControlIssue lastIssue = db.ConsultingRCMDetailRiskControlIssue.OrderByDescending(p => p.NoRef).FirstOrDefault();
            if (lastIssue != null)
            {
                int issueNo = Convert.ToInt32(lastIssue.NoRef.Split('/')[2]) + 1;
                controlIssueNo = Convert.ToString(id + "/ConsultingControlIssue/" + issueNo);
            }
            else
                controlIssueNo = Convert.ToString(id + "/ConsultingControlIssue/" + 1);

            int controlDetailId = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(id)).Select(p => p.ConsultingRCMDetailRiskControlDetailID).FirstOrDefault();
            ViewBag.auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == controlDetailId).ToList();
            ViewBag.controlDetail = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(id)).FirstOrDefault();
            ConsultingRCMDetailRiskControlIssue controlIssue = db.ConsultingRCMDetailRiskControlIssue.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(id)).FirstOrDefault();
            ViewBag.controlIssue = controlIssue;
            if (controlIssue != null)
            {
                ViewBag.findtype = controlIssue.FindingType;
                controlIssueNo = controlIssue.NoRef;
            }
            ViewBag.auditQueryNo = auditQueryNo;
            ViewBag.controlIssueNo = controlIssueNo;
            ConsultingRCMDetailRiskControl rdrc = db.ConsultingRCMDetailRiskControls.Find(id);
            return View(rdrc);
        }

        public ActionResult SaveControlDetail(int controlId, string workDoneDesc, string workDoneRes)
        {
            string username = User.Identity.Name;
            ConsultingRCMDetailRiskControlDetail rdrcd = new ConsultingRCMDetailRiskControlDetail();
            rdrcd.ConsultingRCMDetailRiskControlID = controlId;
            rdrcd.WorkDoneDescription = workDoneDesc;
            rdrcd.WorkDoneResult = workDoneRes;
            db.ConsultingRCMDetailRiskControlDetail.Add(rdrcd);
            db.SaveChanges();
            ConsultingRCMDetailRiskControlDetail condetail = new ConsultingRCMDetailRiskControlDetail();
            auditTransact.CreateAuditTrail("Create", rdrcd.ConsultingRCMDetailRiskControlDetailID, "Consulting Control Detail", condetail, rdrcd, username);
            List<ConsultingAuditQuery> aqs = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null).ToList();
            foreach (var aq in aqs)
            {
                aq.ConsultingRCMDetailRiskControlDetailID = rdrcd.ConsultingRCMDetailRiskControlDetailID;
                db.Entry(aq).State = EntityState.Modified;
            }
            db.SaveChanges();
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [ValidateInput(false)]
        public ActionResult SaveControlIssue(string status,int controlId, string noref, string title, string fact, string cri, string imp, string impactval, string coz, string rec, string managres, string findtype, string picName)
        {
            bool iscreate = false;
            ConsultingRCMDetailRiskControlIssue rdrci = db.ConsultingRCMDetailRiskControlIssue.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();
            string username = User.Identity.Name;
            //ConsultingRCMDetailRiskControlIssue rdrci = new ConsultingRCMDetailRiskControlIssue();
            if (rdrci == null)
            {
                iscreate = true;
                rdrci = new ConsultingRCMDetailRiskControlIssue();
            }
            else
            {
                db.Configuration.ProxyCreationEnabled = false;
            }

            int picId = db.Employees.Where(p => p.Name == picName).Select(p => p.EmployeeID).FirstOrDefault();
            rdrci.ConsultingRCMDetailRiskControlID = controlId;
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
            rdrci.PICID = Convert.ToString(picName);
            //rdrci.Status_App = status;
            if (rdrci.Status_App != "Draft" && status == "Draft")
                rdrci.Status_App = HelperController.GetStatusSendback(db, "Consulting Control Issue", rdrci.Status_App);
            else
                rdrci.Status_App = status;
            //db.ConsultingRCMDetailRiskControlIssue.Add(rdrci);
            SelectList auditSelectList;
            if (iscreate)
            {
                db.ConsultingRCMDetailRiskControlIssue.Add(rdrci);
                //db.SaveChanges();
                //ConsultingRCMDetailRiskControlIssue createissue = new ConsultingRCMDetailRiskControlIssue();
                //auditTransact.CreateAuditTrail("Create", rdrci.ConsultingRCMDetailRiskControlIssueID, "Consulting Control Issue", createissue, rdrci, username);
                //var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
                //auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);    
            }
            else
            {
                //ConsultingRCMDetailRiskControlIssue oldData = db.ConsultingRCMDetailRiskControlIssue.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskControlID.Equals(rdrci.ConsultingRCMDetailRiskControlID)).FirstOrDefault();
                //auditTransact.CreateAuditTrail("Update", rdrci.ConsultingRCMDetailRiskControlID, "Consulting Control Issue", oldData, rdrci, username);
                db.Entry(rdrci).State = EntityState.Modified;
                //db.SaveChanges();
                //var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
                //auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            }

            if (iscreate)
            {
                ConsultingRCMDetailRiskControlIssue createissue = new ConsultingRCMDetailRiskControlIssue();
                auditTransact.CreateAuditTrail("Create", rdrci.ConsultingRCMDetailRiskControlIssueID, "Consulting Control Issue", createissue, rdrci, username);
            }
            db.SaveChanges();
            //ConsultingRCMDetailRiskControlIssue createissue = new ConsultingRCMDetailRiskControlIssue();
            //auditTransact.CreateAuditTrail("Create", rdrci.ConsultingRCMDetailRiskControlIssueID, "Consulting Control Issue", createissue, rdrci, username);
            //var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            //SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [ValidateInput(false)]
        public ActionResult UpdateControlIssue(int controlId, string noref, string title, string fact, string cri, string imp, string impactval, string coz, string rec, string managres, string findtype, string picName, string status)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailRiskControlIssue rdrci = db.ConsultingRCMDetailRiskControlIssue.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();
            rdrci.ConsultingRCMDetailRiskControlID = controlId;
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
            rdrci.PICID = Convert.ToString(picName);
            //rdrci.Status_App = status;
            if (rdrci.Status_App != "Draft" && status == "Draft")
                rdrci.Status_App = HelperController.GetStatusSendback(db, "Consulting Control Issue", rdrci.Status_App);
            else
                rdrci.Status_App = status;
            ConsultingRCMDetailRiskControlIssue oldData = db.ConsultingRCMDetailRiskControlIssue.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskControlID.Equals(rdrci.ConsultingRCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", rdrci.ConsultingRCMDetailRiskControlID, "Consulting Control Issue", oldData, rdrci, username);
            db.Entry(rdrci).State = EntityState.Modified;
            db.SaveChanges();
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        public ActionResult UpdateControlDetail(int controlId, string workDoneDesc, string workDoneRes)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            ConsultingRCMDetailRiskControlDetail rdrcd = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();
            rdrcd.ConsultingRCMDetailRiskControlID = controlId;
            rdrcd.WorkDoneDescription = workDoneDesc;
            rdrcd.WorkDoneResult = workDoneRes;
            ConsultingRCMDetailRiskControlDetail oldData = db.ConsultingRCMDetailRiskControlDetail.AsNoTracking().Where(p => p.ConsultingRCMDetailRiskControlID.Equals(rdrcd.ConsultingRCMDetailRiskControlID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", rdrcd.ConsultingRCMDetailRiskControlID, "Consulting Control Detail", oldData, rdrcd, username);
            db.Entry(rdrcd).State = EntityState.Modified;
            db.SaveChanges();
            List<ConsultingAuditQuery> aqs = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null).ToList();
            foreach (var aq in aqs)
            {
                aq.ConsultingRCMDetailRiskControlDetailID = rdrcd.ConsultingRCMDetailRiskControlDetailID;
                db.Entry(aq).State = EntityState.Modified;
            }
            db.SaveChanges();
            var auditStep = db.ConsultingRCMDetailControlAuditSteps.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).Select(p => new { auditId = p.ConsultingRCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }


        [ValidateInput(false)]
        public ActionResult SubmitAuditQuery(string no, string status,string tanggal, string kepada, string dari, string lampiran, string perihal, string pembuka, string issuedesc, string criteria, string impact, string auditeeclar, string penutup, string folder, int controlId)
        {
            string username = User.Identity.Name;
            ConsultingAuditQuery aq = new ConsultingAuditQuery();
            //int kepadaId = db.Employees.Where(p => p.Name == kepada).Select(p => p.EmployeeID).FirstOrDefault();
            int dariId = db.Employees.Where(p => p.Name == dari).Select(p => p.EmployeeID).FirstOrDefault();
            aq.No = no; aq.Tanggal = (tanggal != String.Empty) ? Convert.ToDateTime(tanggal) : DateTime.Now; aq.Kepada = kepada; aq.Dari = dariId; aq.Lampiran = lampiran;
            aq.Perihal = perihal; aq.Pembuka = pembuka; aq.IssueDesc = issuedesc; aq.Criteria = criteria; aq.Impact = impact;
            aq.AuditeeClarification = auditeeclar; aq.Penutup = penutup; aq.Status = status; aq.Folder = folder;
            db.ConsultingAuditQuery.Add(aq);
            db.SaveChanges();
            ConsultingAuditQuery createquery = new ConsultingAuditQuery();
            auditTransact.CreateAuditTrail("Create", aq.ConsultingAuditQueryID, "Consulting Audit Query", createquery, aq, username);
            var controlDetail = db.ConsultingRCMDetailRiskControlDetail.Where(p => p.ConsultingRCMDetailRiskControlID == controlId).FirstOrDefault();
            var auditQuery = new List<ConsultingAuditQuery>();
            if (controlDetail == null)
            {
                auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null).ToList();
            }
            else
            {
                aq.ConsultingRCMDetailRiskControlDetailID = controlDetail.ConsultingRCMDetailRiskControlDetailID;
                ConsultingAuditQuery audit = db.ConsultingAuditQuery.Find(aq.ConsultingAuditQueryID);
                db.Entry(audit).State = EntityState.Modified;
                db.SaveChanges();
                auditQuery = db.ConsultingAuditQuery.Where(p => p.ConsultingRCMDetailRiskControlDetailID == null || p.ConsultingRCMDetailRiskControlDetailID == controlDetail.ConsultingRCMDetailRiskControlDetailID).ToList();
            } 
            var result = auditQuery.Select(p => new { auditQueryNo = p.No + ";" + p.ConsultingAuditQueryID, auditQueryStatus = p.Status }).ToList();
            SelectList auditQuerySelectList = new SelectList(result, "auditQueryNo", "auditQueryStatus", 0);
            return Json(auditQuerySelectList);
        }

        public JsonResult UploadFile()
        {
            ConsultingAuditQuery auditq = db.ConsultingAuditQuery.OrderByDescending(p => p.ConsultingAuditQueryID).FirstOrDefault();
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

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
        }

        private bool checkCanApprove(int fwId)
        {
            bool result = true;
            ConsultingFieldWork fw = db.ConsultingFieldWork.AsNoTracking().Where(p => p.ConsultingFieldWorkID == fwId).FirstOrDefault();
            List<int> riskIds = db.ConsultingRCMDetailRisks.Where(p => p.ConsultingRCMID == fw.ConsultingRCMID).Select(p => p.ConsultingRCMDetailRiskID).ToList();
            if (riskIds.Count() > 0)
            {
                List<int> controlIds = db.ConsultingRCMDetailRiskControls.Where(p => (riskIds.Contains(p.ConsultingRCMDetailRiskID))).Select(p => p.ConsultingRCMDetailRiskControlID).ToList();
                if (controlIds.Count() > 0)
                {
                    List<string> detailIds = db.ConsultingRCMDetailRiskControlDetail.Where(p => (controlIds.Contains(p.ConsultingRCMDetailRiskControlID))).Select(p => p.ConsultingRCMDetailRiskControlDetailID.ToString()).ToList();
                    if (detailIds.Count() > 0)
                    {
                        List<string> aqStatus = db.ConsultingAuditQuery.Where(p => (detailIds.Contains(p.ConsultingRCMDetailRiskControlDetailID.ToString()))).Select(p => p.Status).ToList();
                        if (aqStatus.Count() > 0)
                        {
                            bool haveAqPending = aqStatus.Any(p => p.StartsWith("Pending"));
                            if (aqStatus.Contains("Draft") || haveAqPending)
                                result = false;
                        }
                    }
                    List<string> issueStatus = db.ConsultingRCMDetailRiskControlIssue.Where(p => (controlIds.Contains(p.ConsultingRCMDetailRiskControlID))).Select(p => p.Status_App).ToList();
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

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingAuditQuery auditQuery = await db.ConsultingAuditQuery.FindAsync(id);
            if (auditQuery == null)
            {
                return HttpNotFound();
            }

            ViewBag.Kepada = auditQuery.Kepada; //.Replace(";", "<br />:&nbsp;");

            IQueryable<Employee> emp = db.Employees.Where(p => p.EmployeeID.Equals(auditQuery.Dari));

            ViewBag.dari = emp.Select(p => p.Name).FirstOrDefault();

            int a = auditQuery.ConsultingRCMDetailRiskControlDetail.ConsultingRCMDetailRiskControl.ConsultingRCMDetailRisk.ConsultingRCMID;
            List<ConsultingFieldWork> sp_print = db.ConsultingFieldWork.Where(p => p.ConsultingRCMID == a).ToList();

            ViewBag.spPrint = sp_print.Select(p => p.ConsultingLetterOfCommand.NomorSP).FirstOrDefault();
            ViewBag.WordDocumentFilename = "AuditQuery";
            return View(auditQuery);
        }

    }
}

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
    public class WalktroughsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private FilesUploadController filesTransact = new FilesUploadController();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        // GET: Walktroughs
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var walktroughs = db.Walktroughs.Where(m =>m.Status =="Approve");
            return View(await walktroughs.ToListAsync());
        }

        // GET: Walktroughs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough = db.Walktroughs.Find(id);
            BusinessProces business = db.BusinessProcess.Find(id);

            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            bool getFiles = filesTransact.getFiles(business.DocumentNo, out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;           

            ViewBag.WalktroughID = business.WalktroughID;
            ViewBag.BPMID = business.BPMID;
            ViewBag.BusinessID = business.BusinessProcesID;
            ViewBag.No = business.DocumentNo;
            ViewBag.Name = business.DocumentName;
            ViewBag.Mark = business.Walktrough.Remarks;


            return View(walktrough);          
        }

        [ExcelDocument]
        public ActionResult Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough = db.Walktroughs.Find(id);
            BusinessProces business = db.BusinessProcess.Find(id);

            List<RiskControlMatrix> rcmList = new List<RiskControlMatrix>();
            //List<RCMDetailRisk> rcmDetailList = new List<RCMDetailRisk>();
            List<RiskControlMatrix> rcms = db.RiskControlMatrixs.Where(p => p.WalktroughID.Equals(walktrough.WalktroughID)).ToList();

            foreach (var busProses in rcms)
            {
                rcmList.Add(busProses);
            }

            //List<RiskControlMatrix> rcmList = new List<RiskControlMatrix>();
            
            List<RCMDetailRiskControl> rcmDetailControlList = new List<RCMDetailRiskControl>();
            List<RCMDetailControlAuditStep> rcmDetailControlAuditStepList = new List<RCMDetailControlAuditStep>();
            List<RCMDetailRiskControlIssue> rcmDetailControlIssueList = new List<RCMDetailRiskControlIssue>();

            //List<RiskControlMatrix> rcms = db.RiskControlMatrixs.Where(p => p.WalktroughID.Equals(walktrough.WalktroughID)).ToList();
            
            /*foreach (var descProses in rcms)
            {   
                rcmList.Add(descProses);
                List<RCMDetailRisk> rcmsss = db.RCMDetailRisks.Where(a => a.RiskControlMatrixID.Equals(descProses.RiskControlMatrixID)).ToList();
                foreach (var desc2 in rcmsss)
                {
                    rcmDetailList.Add(desc2);
                    List<RCMDetailRiskControl> rcmsssControl = db.RCMDetailRiskControls.Where(a => a.RCMDetailRiskID.Equals(desc2.RCMDetailRiskID)).ToList();
                    foreach (var desc3 in rcmsssControl)
                    {
                        rcmDetailControlList.Add(desc3);
                        List<RCMDetailControlAuditStep> rcmsssControlAudit = db.RCMDetailControlAuditSteps.Where(a => a.RCMDetailRiskControlID.Equals(desc3.RCMDetailRiskControlID)).ToList();
                        foreach (var desc4 in rcmsssControlAudit)
                        {
                            rcmDetailControlAuditStepList.Add(desc4);
                        }
                        List<RCMDetailRiskControlIssue> rcmsssControlIssue = db.RCMDetailRiskControlIssue.Where(a => a.RCMDetailRiskControlID.Equals(desc3.RCMDetailRiskControlID)).ToList();
                        foreach (var desc5 in rcmsssControlIssue)
                        {
                            rcmDetailControlIssueList.Add(desc5);
                        }
                    }
                }
            }*/

            ViewBag.descControlIssue = rcmDetailControlIssueList;
            ViewBag.descControlAuditStep = rcmDetailControlAuditStepList;
            ViewBag.descControlRisks = rcmDetailControlList;
            ViewBag.descRisk = rcmList;
            //ViewBag.descProses = rcmList;
            ViewBag.busProses = rcmList;
            ViewBag.WordDocumentFilename = "RCM";
            return View(walktrough);
        }

        
        // GET: Walktroughs/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough =  db.Walktroughs.Find(id);
            BPM bpm = db.BPMs.Find(id);

            //ViewBag.Name = db.BusinessProcess.Find(id);
            if (walktrough == null)
            {
                return HttpNotFound();
            }

            ViewBag.WalktroughID = walktrough.WalktroughID;
            ViewBag.ActivityID = walktrough.ActivityID;

            walktrough.BPM = (from b in db.BPMs
                                         where b.WalktroughID == id
                                         select b).ToList();

            walktrough.RCM = (from b in db.RiskControlMatrixs
                              where b.WalktroughID == id
                              select b).ToList();
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            return View(walktrough);
        }



        // POST: Walktroughs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WalktroughID,LetterOfCommandID,ActivityID,Date_Start,Date_End,Remarks,Status")] Walktrough walktrough)
        {
            if (ModelState.IsValid)
            {
                db.Walktroughs.Add(walktrough);
                TempData["message"] = "Walktrough successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", walktrough.ActivityID);
 
            return View(walktrough);
        }


        public ActionResult CreateBusiness(int? id, int? walkid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough = db.Walktroughs.Find(walkid);
            BusinessProces business = db.BusinessProcess.Find(id);
            BPM bpm = db.BPMs.Find(id);

            //ViewBag.Name = db.BusinessProcess.Find(id);
            if (walktrough == null)
            {
                return HttpNotFound();
            }

            //ViewBag.BusinessProcesID = business.BusinessProcesID;
            ViewBag.WalktroughID = walktrough.WalktroughID;
            ViewBag.ActivityID = walktrough.ActivityID;
            ViewBag.BPMID = bpm.BPMID;
            ViewBag.PrelimID = walktrough.PreliminaryID;
            ViewBag.BPMStatus = bpm.Status;
            ViewBag.WalktroughID = bpm.WalktroughID;
            ViewBag.Name = bpm.Name;

            walktrough.BusinessProces = (from b in db.BusinessProcess
                                         where b.BPMID == id
                                         select b).ToList();

            walktrough.BPM = (from b in db.BPMs
                                         where b.BPMID == id
                                         select b).ToList();
            
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }

            return View(walktrough);

            //return View(walktrough);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateBusiness(IEnumerable<HttpPostedFileBase> files, [Bind(Include = "BusinessProcesID,WalktroughID,DocumentNo,DocumentName,FolderName,BPMID")] BusinessProces businessProces)
        {
            if (ModelState.IsValid)
            {
                HttpServerUtilityBase server = Server;
                int i = 0;
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(businessProces.DocumentNo, i, file, server);
                    }
                }
                string username = User.Identity.Name;

                db.BusinessProcess.Add(businessProces);
                await db.SaveChangesAsync();
                int wallid = Convert.ToInt32(businessProces.WalktroughID);

                BusinessProces bus = new BusinessProces();
                auditTransact.CreateAuditTrail("Create", wallid, "BusinessProcessDetail", bus, businessProces, username);

                TempData["message"] = "BPM detail successfully created!";
                return RedirectToAction("CreateBusiness", new { id = businessProces.BPMID, walkid = businessProces.WalktroughID });
                //return RedirectToAction("Create");
            }

            //ViewBag.WalktroughID = new SelectList(db.Walktroughs, "WalktroughID", "Remarks", businessProces.WalktroughID);
            return View(businessProces);
        }

        public ActionResult EditBusiness(int? id, int? walkid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough = db.Walktroughs.Find(walkid);
            BusinessProces business = db.BusinessProcess.Find(id);
            BPM bpm = db.BPMs.Find(id);

            if (walktrough == null)
            {
                return HttpNotFound();
            }
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            var getFiles = filesTransact.getFiles(business.DocumentNo, out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;

            ViewBag.WalktroughID = business.WalktroughID;
            ViewBag.BPMID = business.BPMID;
            ViewBag.BusinessProcesID = business.BusinessProcesID;
            ViewBag.DocumentNo = business.DocumentNo;
            ViewBag.DocumentName = business.DocumentName;
            ViewBag.FolderName = business.FolderName;

            walktrough.BusinessProces = (from b in db.BusinessProcess
                                         where b.BPMID == id
                                         select b).ToList();

            return View(walktrough);            
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBusiness(IEnumerable<HttpPostedFileBase> files, [Bind(Include = "BusinessProcesID,WalktroughID,DocumentNo,DocumentName,FolderName,BPMID")] BusinessProces businessProces)
        {
            if (ModelState.IsValid)
            {
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
                    bool getFiles = filesTransact.getFiles(businessProces.DocumentNo, out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(businessProces.DocumentNo, server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(businessProces.DocumentNo, i, file, server);
                    }
                }
                Session.Remove("Images");

                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                BusinessProces oldData = db.BusinessProcess.AsNoTracking().Where(p => p.BusinessProcesID.Equals(businessProces.BusinessProcesID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update",  Convert.ToInt32(businessProces.WalktroughID), "BusinessProcessDetail", oldData, businessProces, username);
                db.Entry(businessProces).State = EntityState.Modified;
                await db.SaveChangesAsync();
                db.SaveChanges();

                TempData["message"] = "BPM detail successfully updated!";
                return RedirectToAction("CreateBusiness", new { id = businessProces.BPMID, walkid = businessProces.WalktroughID });
               
            }
            
            return View(businessProces);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateBPM(IEnumerable<HttpPostedFileBase> files, [Bind(Include = "BPMID,WalktroughID,Name")] BPM bpm, string submit, int WalktroughID)
        {
            if (ModelState.IsValid)
            {
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save" || submit == "Send Back")
                    bpm.Status = "Draft";
                else if (submit == "Approve")
                    bpm.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    bpm.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    bpm.Status = "Pending for Approve by" + user;
                }

                db.BPMs.Add(bpm);
                await db.SaveChangesAsync();
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "bpm";
                rrm.Description = page + bpm.BPMID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();
                string username = User.Identity.Name;
                int wallid = Convert.ToInt32(bpm.WalktroughID);
                BPM bp = new BPM();
                auditTransact.CreateAuditTrail("Create", wallid, "BPM", bp, bpm, username);
                TempData["message"] = "BPM successfully created!";
                return RedirectToAction("CreateBusiness", new { id = bpm.BPMID, walkid = WalktroughID });
                //return RedirectToAction("Create");
            }
            return View(bpm);
        }

        private bool sentEmailBPM(BPM bpm, string user, int WalktroughID)
        {
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
                        string emailContent = "Dear {0}, <BR/><BR/>BPM : {1} need your approval. Please click on this <a href=\"{2}\" title=\"BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string urlBPM = baseUrl + "/Walktroughs/CreateBusiness/" + bpm.BPMID + "?walkid=" + WalktroughID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, bpm.Name, emailContent, urlBPM);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailBPM(string userToSentEmail, BPM bpm, int WalktroughID)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>BPM : {1} need your approval. Please click on this <a href=\"{2}\" title=\"BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string urlBPM = baseUrl + "/Walktroughs/CreateBusiness/" + bpm.BPMID + "?walkid=" + WalktroughID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, bpm.Name, emailContent, urlBPM);
            }
            return true;
        }

        private bool sentEmailRCM(RiskControlMatrix risk, string user, int walktroughId)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            List<string> CIAUserIdsPengawas = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
            List<Employee> CIAEmpListPengawas = new List<Employee>();
            if (CIAUserIdsPengawas.Count() > 0)
            {
                var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIdsPengawas.Contains(p.Id))).ToList();
                foreach (var CIAUser in CIAUsers)
                {

                    Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();
                    if (emp != null)
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>RCM : {1} need your approval. Please click on this <a href=\"{2}\" title=\"RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/Walktroughs/Create/" + walktroughId;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, risk.Objectives, emailContent, url);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailRCM(string userToSentEmail, RiskControlMatrix risk, int walktroughId)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>RCM : {1} need your approval. Please click on this <a href=\"{2}\" title=\"RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = baseUrl + "/Walktroughs/Create/" + walktroughId;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, risk.Objectives, emailContent, url);
            }
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateBPM(string submit, int BPMID,int WalktroughID,int PrelimID)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            BPM oldData = db.BPMs.AsNoTracking().Where(p => p.BPMID.Equals(BPMID)).FirstOrDefault();
            BPM bpm = db.BPMs.Find(BPMID);
            int? prelimId = PrelimID;
            Preliminary prelim = db.Preliminaries.Find(prelimId);

            string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
            if (submit == "Save")
                bpm.Status = "Draft";
            else if(submit == "Send Back")
                bpm.Status = HelperController.GetStatusSendback(db, "BPM", bpm.Status);
            else if (submit == "Approve")
                bpm.Status = "Approve";
            else if (submit == "Submit For Review By" + user)
                bpm.Status = "Pending for Review by" + user;
            else if (submit == "Submit For Approve By" + user)
            {
                bpm.Status = "Pending for Approve by" + user;
                string userToSentEmail = String.Empty;
                if (user.Trim() == "CIA")
                {
                    userToSentEmail = prelim.PICID;
                    if (userToSentEmail != null)
                        sentSingleEmailBPM(userToSentEmail, bpm, WalktroughID);
                    else
                        sentEmailBPM(bpm, user.Trim(), WalktroughID);
                }
                else if (user.Trim() == "Pengawas")
                {
                    userToSentEmail = prelim.SupervisorID;
                    if (userToSentEmail != null)
                        sentSingleEmailBPM(userToSentEmail, bpm, WalktroughID);
                    else
                        sentEmailBPM(bpm, user.Trim(), WalktroughID);
                }
                else if (user.Trim() == "Ketua Tim")
                {
                    userToSentEmail = prelim.TeamLeaderID;
                    if (userToSentEmail != null)
                        sentSingleEmailBPM(userToSentEmail, bpm, WalktroughID);
                    else
                        sentEmailBPM(bpm, user.Trim(), WalktroughID);
                }
                else if (user.Trim() == "Member")
                {
                    userToSentEmail = prelim.MemberID;
                    if (userToSentEmail != null)
                        sentSingleEmailBPM(userToSentEmail, bpm, WalktroughID);
                    else
                        sentEmailBPM(bpm, user.Trim(), WalktroughID);
                }
            }

                auditTransact.CreateAuditTrail("Update", bpm.WalktroughID, "BusinessProces", oldData, bpm, username);
                db.Entry(bpm).State = EntityState.Modified;               
                await db.SaveChangesAsync();
                TempData["message"] = "BPM successfully updated!";
                return RedirectToAction("Create", new { id = bpm.WalktroughID });
        }


        public async Task<ActionResult> DeleteBPM(int id, int walkId)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            List<BusinessProces> bpList = db.BusinessProcess.Where(p => p.BPMID == id).ToList();
            if (bpList.Count() > 0)
            {
                foreach (var bp in bpList)
                {
                    db.BusinessProcess.Remove(bp);
                }
            }
            BPM bpm = db.BPMs.Find(id);
            db.BPMs.Remove(bpm);
            await db.SaveChangesAsync();
            BPM newData = new BPM();
            auditTransact.CreateAuditTrail("Delete", id, "BPM", bpm, newData, username);
            TempData["message"] = "BPM " +bpm.Name+ " successfully deleted!";
            return RedirectToAction("Create", new { id = walkId });
        }

        public async Task<ActionResult> EditBPM(int IdBpmedit, string BPMEdit, int WalktroughID)
        {
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;

            BPM bpm = db.BPMs.Find(IdBpmedit);
            bpm.Name = BPMEdit;
            
            db.Entry(bpm).State = EntityState.Modified;
            await db.SaveChangesAsync();

            BPM newData = new BPM();
            
            auditTransact.CreateAuditTrail("Update", IdBpmedit, "BPM", bpm, newData, username);
            TempData["message"] = "BPM " + bpm.Name + " successfully updated!";
            return RedirectToAction("Create", new { id = WalktroughID });
        }
        // GET: Walktroughs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough = await db.Walktroughs.FindAsync(id);
            if (walktrough == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", walktrough.ActivityID);
            ViewBag.PreliminaryID = new SelectList(db.LetterOfCommands, "PreliminaryID", "NomorPreliminarySurvey", walktrough.PreliminaryID);
            return View(walktrough);
        }

        // POST: Walktroughs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "WalktroughID,LetterOfCommandID,ActivityID,Date_Start,Date_End,Remarks,Status")] Walktrough walktrough)
        {
            if (ModelState.IsValid)
            {

                db.Entry(walktrough).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["message"] = "Walktrough successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", walktrough.ActivityID);
            ViewBag.PreliminaryID = new SelectList(db.LetterOfCommands, "PreliminaryID", "NomorPreliminarySurvey", walktrough.PreliminaryID);
            return View(walktrough);
        }

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
        }

        // GET: Walktroughs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Walktrough walktrough = db.Walktroughs.Find(id);
            BusinessProces business = db.BusinessProcess.Find(id);

            ViewBag.WalktroughID = business.WalktroughID;
            ViewBag.BPMID = business.BPMID;
            ViewBag.BusinessID = business.BusinessProcesID;
            ViewBag.No = business.DocumentNo;
            ViewBag.Name = business.DocumentName;
            ViewBag.Mark = business.Walktrough.Remarks;


            return View(walktrough);   
        }

        // POST: Walktroughs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            BusinessProces business = await db.BusinessProcess.FindAsync(id);

            BusinessProces emp = new BusinessProces();
            db.BusinessProcess.Remove(business);
            await db.SaveChangesAsync();
            auditTransact.CreateAuditTrail("Delete", Convert.ToInt32(business.WalktroughID), "BusinessProcesDetail", business, emp, username);
            TempData["message"] = "BPM detail successfully deleted!";
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

        #region RCM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRCM(IEnumerable<HttpPostedFileBase> files, [Bind(Include = "RiskControlMatrixID,BusinessProcesID,SubBusinessProcess,Objectives,WalktroughID")] RiskControlMatrix rcm, string bpms, string submit)
        {
            if (ModelState.IsValid)
            {   
                var bpm = db.BPMs.Where(p => p.Name.Equals(bpms)).FirstOrDefault();
                if (bpm != null && bpm.Status == "Approve")
                {
                    int bpmid = bpm.BPMID;
                    rcm.BusinessProcesID = bpmid;
                    string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    rcm.Status = "Draft";
                else if(submit == "Send Back")
                    rcm.Status = "Draft";
                else if (submit == "Approve")
                    rcm.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    rcm.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    bpm.Status = "Pending for Approve by" + user;
                }
                    db.RiskControlMatrixs.Add(rcm);
                    await db.SaveChangesAsync();
                    await db.SaveChangesAsync();
                    ReviewRelationMaster rrm = new ReviewRelationMaster();
                    string page = "rcm";
                    rrm.Description = page + rcm.RiskControlMatrixID;
                    db.ReviewRelationMasters.Add(rrm);
                    db.SaveChanges();
                    string username = User.Identity.Name;
                    int wallid = Convert.ToInt32(rcm.WalktroughID);

                    RiskControlMatrix rc = new RiskControlMatrix();
                    auditTransact.CreateAuditTrail("Create", wallid, "RiskControlMatrix", rc, rcm, username);
                    //return RedirectToAction("Create");
                }
            }
            return RedirectToAction("Create", new { id = rcm.WalktroughID });
        }

        public ActionResult DetailRCM(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskControlMatrix rcm = db.RiskControlMatrixs.Find(id);
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == id).Select(p => new {riskId = p.RCMDetailRiskID, riskName = p.RiskName}).ToList();
            ViewBag.RCMStatus = rcm.Status;
            ViewBag.prelimID = rcm.Walktrough.PreliminaryID;
            ViewBag.RDR = new SelectList(rdr, "riskId", "riskName"); ;
            return View(rcm);
        }

        [HttpPost]
        public ActionResult GetAuditStepByRiskControlId(int controlId)
        {
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult GetControlsByRiskId(int riskId)
        {
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult UpdateRCMStatus(int rcmId, string status, int walktroughId, int prelimID)
        {
            //var RCM = db.RiskControlMatrixs.Find(rcmId);
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            RiskControlMatrix oldData = db.RiskControlMatrixs.AsNoTracking().Where(p => p.RiskControlMatrixID.Equals(rcmId)).FirstOrDefault();
            RiskControlMatrix risk = db.RiskControlMatrixs.Find(rcmId);
            Preliminary prelim = db.Preliminaries.Find(prelimID);
            if (status == "Draft")
            { risk.Status = HelperController.GetStatusSendback(db, "RCM", risk.Status); }
            else
            { risk.Status = status; }
            
            
            auditTransact.CreateAuditTrail("Update", risk.WalktroughID, "RiskControlMatrix", oldData, risk, username);
            db.Entry(risk).State = EntityState.Modified;
            db.SaveChanges();
            if (status.Contains("Pending for Approve"))
            {
                string user = status.Split('y')[1];
                string userToSentEmail = String.Empty;
                if (user.Trim() == "CIA")
                {
                    userToSentEmail = prelim.PICID;
                    if (userToSentEmail != null)
                        sentSingleEmailRCM(userToSentEmail, risk, walktroughId);
                    else
                        sentEmailRCM(risk, user.Trim(), walktroughId);
                }
                else if (user.Trim() == "Pengawas")
                {
                    userToSentEmail = prelim.SupervisorID;
                    if (userToSentEmail != null)
                        sentSingleEmailRCM(userToSentEmail, risk, walktroughId);
                    else
                        sentEmailRCM(risk, user.Trim(), walktroughId);
                }
                else if (user.Trim() == "Ketua Tim")
                {
                    userToSentEmail = prelim.TeamLeaderID;
                    if (userToSentEmail != null)
                        sentSingleEmailRCM(userToSentEmail, risk, walktroughId);
                    else
                        sentEmailRCM(risk, user.Trim(), walktroughId);
                }
                else if (user.Trim() == "Member")
                {
                    userToSentEmail = prelim.MemberID;
                    if (userToSentEmail != null)
                        sentSingleEmailRCM(userToSentEmail, risk, walktroughId);
                    else
                        sentEmailRCM(risk, user.Trim(), walktroughId);
                }
            }

            var listStatus = db.RiskControlMatrixs.Where(p => p.WalktroughID.Equals(walktroughId)).Select(p => new { RCMStatus = p.Status }).ToList();
            SelectList listStatusSelectList = new SelectList(listStatus, "RCMStatus", "RCMStatus", 0);
            return Json(listStatusSelectList);
        }

        [HttpPost]
        public ActionResult SaveRisk(int rcmId, string riskName)
        {
            RCMDetailRisk newRdr = new RCMDetailRisk();
            newRdr.RiskControlMatrixID = rcmId;
            newRdr.RiskName = riskName;
            newRdr.Status = "Pending";
            db.RCMDetailRisks.Add(newRdr);
            db.SaveChanges();
            string username = User.Identity.Name;
            int wallid = db.RiskControlMatrixs.Where(p=>p.RiskControlMatrixID.Equals(rcmId)).Select(p=>p.WalktroughID).FirstOrDefault();
            RCMDetailRisk ri = new RCMDetailRisk();
            auditTransact.CreateAuditTrail("Create", wallid, "RCMDetailRisk", ri, newRdr, username);
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult UpdateRisk(int rcmId, int riskId, string riskName)
        {
            string username = User.Identity.Name;
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(rcmId)).Select(p => p.WalktroughID).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRisk oldData = db.RCMDetailRisks.AsNoTracking().Where(p => p.RCMDetailRiskID.Equals(riskId)).FirstOrDefault();           
            RCMDetailRisk Rdr = db.RCMDetailRisks.Find(riskId);
            Rdr.RiskName = riskName;
            db.Entry(Rdr).State = EntityState.Modified;
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Update", wallid, "RCMDetailRisk", oldData, Rdr, username);
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult DeleteRisk(int rcmId, int riskId)
        {
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(rcmId)).Select(p => p.WalktroughID).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            RCMDetailRisk Rdr = db.RCMDetailRisks.Find(riskId);
            db.RCMDetailRisks.Remove(Rdr);
            db.SaveChanges();
            RCMDetailRisk rc = new RCMDetailRisk();
            auditTransact.CreateAuditTrail("Delete", wallid, "RCMDetailRisk", Rdr, rc, username);
            var rdr = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).Select(p => new { riskId = p.RCMDetailRiskID, riskName = p.RiskName }).ToList();
            SelectList riskSelectList = new SelectList(rdr, "riskId", "riskName"); ;
            return Json(riskSelectList);
        }

        [HttpPost]
        public ActionResult SaveControl(int riskId, string controlName)
        {
            RCMDetailRiskControl newControl = new RCMDetailRiskControl();
            newControl.RCMDetailRiskID = riskId;
            newControl.ControlName = controlName;
            newControl.ReviewMasterID = 0;
            newControl.DueDate = DateTime.Now;
            newControl.CloseDate = DateTime.Now;
            newControl.Status = "Pending";
            db.RCMDetailRiskControls.Add(newControl);
            db.SaveChanges();
            string username = User.Identity.Name;
            int riskid = db.RCMDetailRisks.Where(p => p.RCMDetailRiskID.Equals(riskId)).Select(p => p.RCMDetailRiskID).FirstOrDefault();
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(riskid)).Select(p => p.WalktroughID).FirstOrDefault();
            RCMDetailRiskControl ri = new RCMDetailRiskControl();
            auditTransact.CreateAuditTrail("Create", wallid, "RCMDetailRiskControl", ri, newControl, username);
            ReviewRelationMaster rrm = new ReviewRelationMaster();
            string page = "control";
            rrm.Description = page + newControl.RCMDetailRiskControlID;
            db.ReviewRelationMasters.Add(rrm);
            db.SaveChanges();
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult UpdateControl(int riskId, int controlId, string controlName)
        {
            string username = User.Identity.Name;
            int riskid = db.RCMDetailRisks.Where(p => p.RCMDetailRiskID.Equals(riskId)).Select(p => p.RCMDetailRiskID).FirstOrDefault();
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(riskid)).Select(p => p.WalktroughID).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControl oldData = db.RCMDetailRiskControls.AsNoTracking().Where(p => p.RCMDetailRiskControlID.Equals(controlId)).FirstOrDefault();     
            RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
            control.ControlName = controlName;
            db.Entry(control).State = EntityState.Modified;
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Update", wallid, "RCMDetailRiskControl", oldData, control, username);
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult DeleteControl(int riskId, int controlId)
        {
            string username = User.Identity.Name;
            int riskid = db.RCMDetailRisks.Where(p => p.RCMDetailRiskID.Equals(riskId)).Select(p => p.RCMDetailRiskID).FirstOrDefault();
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(riskid)).Select(p => p.WalktroughID).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(controlId);
            db.RCMDetailRiskControls.Remove(control);
            db.SaveChanges();
            RCMDetailRiskControl rc = new RCMDetailRiskControl();
            auditTransact.CreateAuditTrail("Delete", wallid, "RCMDetailRiskControl", control, rc, username);
            var riskControl = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).Select(p => new { controlId = p.RCMDetailRiskControlID, controlName = p.ControlName }).ToList(); ;
            SelectList riskControlSelectList = new SelectList(riskControl, "controlId", "ControlName", 0);
            return Json(riskControlSelectList);
        }

        [HttpPost]
        public ActionResult SaveAudit(int controlId, string auditName)
        {
            RCMDetailControlAuditStep newAudit = new RCMDetailControlAuditStep();
            newAudit.RCMDetailRiskControlID = controlId;
            newAudit.AuditStepName = auditName;
            newAudit.Status = "Pending";
            db.RCMDetailControlAuditSteps.Add(newAudit);
            db.SaveChanges();
            string username = User.Identity.Name;
            int deriskid = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskControlID.Equals(controlId)).Select(p => p.RCMDetailRiskID).FirstOrDefault();
            int riskids = db.RCMDetailRisks.Where(p => p.RCMDetailRiskID.Equals(deriskid)).Select(p => p.RiskControlMatrixID).FirstOrDefault();
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(riskids)).Select(p => p.WalktroughID).FirstOrDefault();
            RCMDetailControlAuditStep ri = new RCMDetailControlAuditStep();
            auditTransact.CreateAuditTrail("Create", wallid, "RCMDetailControlAuditStep", ri, newAudit, username);
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult UpdateAudit(int controlId, string auditName)
        {
            string username = User.Identity.Name;
            int deriskid = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskControlID.Equals(controlId)).Select(p => p.RCMDetailRiskID).FirstOrDefault();
            int riskids = db.RCMDetailRisks.Where(p => p.RCMDetailRiskID.Equals(deriskid)).Select(p => p.RiskControlMatrixID).FirstOrDefault();
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(riskids)).Select(p => p.WalktroughID).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            int auditId = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => p.RCMDetailControlAuditStepID).FirstOrDefault();
            RCMDetailControlAuditStep oldData = db.RCMDetailControlAuditSteps.AsNoTracking().Where(p => p.RCMDetailControlAuditStepID.Equals(auditId)).FirstOrDefault();  
            RCMDetailControlAuditStep audit = db.RCMDetailControlAuditSteps.Find(auditId);
            audit.AuditStepName = auditName;
            db.Entry(audit).State = EntityState.Modified;
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Update", wallid, "RCMDetailControlAuditStep", oldData, audit, username);
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        [HttpPost]
        public ActionResult DeleteAudit(int controlId)
        {
            string username = User.Identity.Name;
            int deriskid = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskControlID.Equals(controlId)).Select(p => p.RCMDetailRiskID).FirstOrDefault();
            int riskids = db.RCMDetailRisks.Where(p => p.RCMDetailRiskID.Equals(deriskid)).Select(p => p.RiskControlMatrixID).FirstOrDefault();
            int wallid = db.RiskControlMatrixs.Where(p => p.RiskControlMatrixID.Equals(riskids)).Select(p => p.WalktroughID).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            int auditId = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => p.RCMDetailControlAuditStepID).FirstOrDefault();
            RCMDetailControlAuditStep audit = db.RCMDetailControlAuditSteps.Find(auditId);
            db.RCMDetailControlAuditSteps.Remove(audit);
            db.SaveChanges();
            RCMDetailControlAuditStep rc = new RCMDetailControlAuditStep();
            auditTransact.CreateAuditTrail("Delete", wallid, "RCMDetailControlAuditStep", audit, rc, username);
            var auditStep = db.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).Select(p => new { auditId = p.RCMDetailControlAuditStepID, auditName = p.AuditStepName }).ToList(); ;
            SelectList auditSelectList = new SelectList(auditStep, "auditId", "auditName", 0);
            return Json(auditSelectList);
        }

        public ActionResult AutocompleteBPM(string term, int WalktroughID)
        {
            var items = db.BPMs.Where(p => p.WalktroughID.Equals(WalktroughID) && p.Status.Equals("Approve")).Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}

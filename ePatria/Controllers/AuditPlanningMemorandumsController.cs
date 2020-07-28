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
using System.IO;
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Controllers
{
    [Authorize()]
    public class AuditPlanningMemorandumsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private FilesUploadController filesTransact = new FilesUploadController();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        // GET: AuditPlanningMemorandums
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var auditPlanningMemorandums = db.AuditPlanningMemorandums.Include(a => a.Activity).Include(a => a.Preliminary);
            return View(await auditPlanningMemorandums.ToListAsync());
        }

        // GET: AuditPlanningMemorandums/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuditPlanningMemorandum auditPlanningMemorandum = db.AuditPlanningMemorandums.Find(id);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            bool getFiles = filesTransact.getFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;
            if (auditPlanningMemorandum == null)
            {
                return HttpNotFound();
            }
            //ViewBag.MemberName = db.Employees.Where(p => p.EmployeeID.Equals(auditPlanningMemorandum.MemberID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.SupervisorName = auditPlanningMemorandum.SupervisorID;
            ViewBag.TeamLeaderName = auditPlanningMemorandum.TeamLeaderID;
            ViewBag.PICIDName = auditPlanningMemorandum.PICID;
            ViewBag.MemberName = auditPlanningMemorandum.MemberID;
            ViewBag.NomerPrelim = db.Preliminaries.Where(p => p.PreliminaryID.Equals(auditPlanningMemorandum.PreliminaryID)).Select(p => p.NomorPreliminarySurvey).FirstOrDefault();

            


            return View(auditPlanningMemorandum);
        }

        // GET: AuditPlanningMemorandums/Create
        public ActionResult Create()
        {
            AnnualPlanningServices activity = new AnnualPlanningServices();
            var activities = activity.GetAnnualPlanning().Where(p => p.Approval_Status == "Approve");

            IEnumerable<SelectListItem> activityid = activities
            .Select(d => new SelectListItem
            {
                Value = d.AnnualPlanningID.ToString(),
                Text = d.Activity.Name

            });


            string apmno = String.Empty;

            AuditPlanningMemorandum apm = db.AuditPlanningMemorandums.OrderByDescending(p => p.AuditPlanningMemorandumID).FirstOrDefault();
            if (apm != null)
            {
                int nomerapm = Convert.ToInt32(apm.NomerAPM.Split('/')[1]) + 1;
                apmno = Convert.ToString("APM/" + nomerapm);
            }
            else
                apmno = Convert.ToString("APM/" + 1);

            ViewBag.apmnomer = apmno;
            return View();
        }

        // POST: AuditPlanningMemorandums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string nomerapm, string membernull, string enggname, EngagementActivity engga, AuditPlanningMemorandum audit, [Bind(Include = "AuditPlanningMemorandumID,NomerAPM,PreliminaryID,Date_Start,Date_End,Status,ActivityID,TujuanAudit,RuangLingkupAudit,MetodologiAudit,DataDanDokumen,AttachmentMasterID,EntryMeetingDateStart,EntryMeetingDateEnd,WalktroughDateStart,WalktroughDateEnd,FieldWorkDateStart,FieldWorkDateEnd,ExitMeetingDateStart,ExitMeetingDateEnd,LHADateStart,LHADateEnd,PICID,SupervisorID,TeamLeaderID,MemberID,ReviewMasterID,Kesimpulan,ActualEngagement")] AuditPlanningMemorandum auditPlanningMemorandum, IEnumerable<HttpPostedFileBase> files, string submit, string[] members, string supervisor, string pic, string leader, string nomer, string activid)
        {

            if (ModelState.IsValid)
                {
                HttpServerUtilityBase server = Server;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    auditPlanningMemorandum.Status = "Draft";
                else if (submit == "Send Back")
                    HelperController.GetStatusSendback(db, "Audit Planning Memorandum", auditPlanningMemorandum.Status);
                else if (submit == "Approve")
                    auditPlanningMemorandum.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    auditPlanningMemorandum.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    auditPlanningMemorandum.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = auditPlanningMemorandum.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = auditPlanningMemorandum.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = auditPlanningMemorandum.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = auditPlanningMemorandum.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                }

                foreach (var mem in members)
                {
                    auditPlanningMemorandum.MemberID += mem + ";";
                }

                IQueryable<Preliminary> prel = db.Preliminaries.Where(p => p.NomorPreliminarySurvey.Equals(nomer));
                int preID = 0;
                if (prel.Count() > 0 || prel != null)
                    preID = prel.Select(p => p.PreliminaryID).FirstOrDefault();
                auditPlanningMemorandum.PreliminaryID = preID;            
                auditPlanningMemorandum.SupervisorID = supervisor;
                auditPlanningMemorandum.TeamLeaderID = leader;
                auditPlanningMemorandum.PICID = pic;

                IQueryable<Activity> act = db.Activities.Where(p => p.Name.Equals(activid));
                int actid = 0;
                if (act.Count() > 0 || act != null)
                    actid = act.Select(p => p.ActivityID).FirstOrDefault();
                auditPlanningMemorandum.ActivityID = actid;
                auditPlanningMemorandum.NomerAPM = nomerapm;
                db.AuditPlanningMemorandums.Add(auditPlanningMemorandum);
                db.SaveChanges();
                int i = 0;
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(auditPlanningMemorandum.NomerAPM.Replace("/", ""), i, file, server);
                    }
                }
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "apm";
                rrm.Description = page + auditPlanningMemorandum.AuditPlanningMemorandumID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();
                string username = User.Identity.Name;
                AuditPlanningMemorandum au = new AuditPlanningMemorandum();
                auditTransact.CreateAuditTrail("Create", auditPlanningMemorandum.AuditPlanningMemorandumID, "AuditPlanningMemorandum", au, auditPlanningMemorandum, username);

                TempData["message"] = "Audit Planning Memorandum successfully created!";
                return RedirectToAction("Index");
            }
            return View(auditPlanningMemorandum);
        }

        // GET: AuditPlanningMemorandums/Edit/5
        public ActionResult Edit(int? id)
         {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuditPlanningMemorandum auditPlanningMemorandum = db.AuditPlanningMemorandums.Find(id);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            var getFiles = filesTransact.getFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;
            if (auditPlanningMemorandum == null)
            {
                return HttpNotFound();
            }           

            PreliminaryServices no = new PreliminaryServices();
            var noap = no.GetPreliminary().Where(p => p.PreliminaryID == auditPlanningMemorandum.PreliminaryID );

            var memberId = db.AuditPlanningMemorandums.Find(auditPlanningMemorandum.AuditPlanningMemorandumID).MemberID;
            ViewBag.memberName = memberId;

            ViewBag.activid = db.Activities.Where(p => p.ActivityID.Equals(auditPlanningMemorandum.ActivityID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.supervisor = auditPlanningMemorandum.SupervisorID;
            ViewBag.leader = auditPlanningMemorandum.TeamLeaderID;
            ViewBag.pic = auditPlanningMemorandum.PICID;
            ViewBag.member = auditPlanningMemorandum.MemberID;

            ViewBag.nomer = db.Preliminaries.Where(p => p.PreliminaryID.Equals(auditPlanningMemorandum.PreliminaryID)).Select(p => p.NomorPreliminarySurvey).FirstOrDefault();
            Preliminary pre = db.Preliminaries.Where(p => p.PreliminaryID.Equals(auditPlanningMemorandum.PreliminaryID)).FirstOrDefault();
            ViewBag.enggname = pre.EngagementActivity.Name;
            return View(auditPlanningMemorandum);
        }

        // POST: AuditPlanningMemorandums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string memberName, string enggname, string nomer, [Bind(Include = "AuditPlanningMemorandumID,NomerAPM,PreliminaryID,Date_Start,Date_End,Status,ActivityID,TujuanAudit,RuangLingkupAudit,MetodologiAudit,DataDanDokumen,AttachmentMasterID,EntryMeetingDateStart,EntryMeetingDateEnd,WalktroughDateStart,WalktroughDateEnd,FieldWorkDateStart,FieldWorkDateEnd,ExitMeetingDateStart,ExitMeetingDateEnd,LHADateStart,LHADateEnd,PICID,SupervisorID,TeamLeaderID,MemberID,ReviewMasterID,Kesimpulan,ActualEngagement")] AuditPlanningMemorandum auditPlanningMemorandum, IEnumerable<HttpPostedFileBase> files, string[] members, string pic, string supervisor, string leader, string submit, EngagementActivity engga, string activid)
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
                    bool getFiles = filesTransact.getFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(auditPlanningMemorandum.NomerAPM.Replace("/", ""), i, file, server);
                    }
                }
                Session.Remove("Images");

                    IQueryable<Preliminary> pre = db.Preliminaries.Where(p => p.NomorPreliminarySurvey.Equals(nomer));
                    int preId = 0;
                    if (pre.Count() > 0 || pre != null)
                        preId = pre.Select(p => p.PreliminaryID).FirstOrDefault();
                    auditPlanningMemorandum.PreliminaryID = preId;

                    IQueryable<Activity> act = db.Activities.Where(p => p.Name.Equals(activid));
                    int actid = 0;
                    if (act.Count() > 0 || act != null)
                        actid = act.Select(p => p.ActivityID).FirstOrDefault();
                    auditPlanningMemorandum.ActivityID = actid;

                    foreach (var mem in members)
                    {
                        auditPlanningMemorandum.MemberID += mem + ";";
                    }

                    auditPlanningMemorandum.SupervisorID = supervisor;

                    auditPlanningMemorandum.TeamLeaderID = leader;

                    auditPlanningMemorandum.PICID = pic;
                    string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                    if (submit == "Save")
                        auditPlanningMemorandum.Status = "Draft";
                    else if (submit == "Send Back")
                        HelperController.GetStatusSendback(db, "Audit Planning Memorandum", auditPlanningMemorandum.Status);
                    else if (submit == "Approve")
                        auditPlanningMemorandum.Status = "Approve";
                    else if (submit == "Submit For Review By" + user)
                        auditPlanningMemorandum.Status = "Pending for Review by" + user;
                    else if (submit == "Submit For Approve By" + user)
                    {
                        auditPlanningMemorandum.Status = "Pending for Approve by" + user;
                        string userToSentEmail = String.Empty;
                        if (user.Trim() == "CIA")
                        {
                            userToSentEmail = auditPlanningMemorandum.PICID;
                            if (userToSentEmail != null)
                                sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                            else
                                sentEmailAPM(auditPlanningMemorandum, user.Trim());
                        }
                        else if (user.Trim() == "Pengawas")
                        {
                            userToSentEmail = auditPlanningMemorandum.SupervisorID;
                            if (userToSentEmail != null)
                                sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                            else
                                sentEmailAPM(auditPlanningMemorandum, user.Trim());
                        }
                        else if (user.Trim() == "Ketua Tim")
                        {
                            userToSentEmail = auditPlanningMemorandum.TeamLeaderID;
                            if (userToSentEmail != null)
                                sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                            else
                                sentEmailAPM(auditPlanningMemorandum, user.Trim());
                        }
                        else if (user.Trim() == "Member")
                        {
                            userToSentEmail = auditPlanningMemorandum.MemberID;
                            if (userToSentEmail != null)
                                sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                            else
                                sentEmailAPM(auditPlanningMemorandum, user.Trim());
                        }
                    }

                    db.Entry(auditPlanningMemorandum).State = EntityState.Modified;
                    string username = User.Identity.Name;
                    db.Configuration.ProxyCreationEnabled = false;
                    AuditPlanningMemorandum oldData = db.AuditPlanningMemorandums.AsNoTracking().Where(p => p.AuditPlanningMemorandumID.Equals(auditPlanningMemorandum.AuditPlanningMemorandumID)).FirstOrDefault();
                    auditTransact.CreateAuditTrail("Update", auditPlanningMemorandum.AuditPlanningMemorandumID, "AuditPlanningMemorandum", oldData, auditPlanningMemorandum, username);
                    db.SaveChanges();
                TempData["message"] = "Audit Planning Memorandum successfully updated!";
                return RedirectToAction("Index");
            }
            //ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", auditPlanningMemorandum.ActivityID);
            //ViewBag.PreliminaryID = new SelectList(db.Preliminaries, "PreliminaryID", "Type", auditPlanningMemorandum.PreliminaryID);
            return View(auditPlanningMemorandum);
        }

        private bool sentEmailAPM(AuditPlanningMemorandum auditPlanningMemorandum, string user)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
            List<Employee> CIAEmpList = new List<Employee>();
            if (CIAUserIds.Count() > 0)
            {
                var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                foreach (var CIAUser in CIAUsers)
                {
                    Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();
                    if (emp != null)
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>APM : {1} need your approval. Please click on this <a href=\"{2}\" title=\"APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string urll = baseUrl + "/AuditPlanningMemorandums/Details/" + auditPlanningMemorandum.AuditPlanningMemorandumID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, auditPlanningMemorandum.NomerAPM, emailContent, urll);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailAPM(string userToSentEmail, AuditPlanningMemorandum auditPlanningMemorandum)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>APM : {1} need your approval. Please click on this <a href=\"{2}\" title=\"APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string urll = baseUrl + "/AuditPlanningMemorandums/Details/" + auditPlanningMemorandum.AuditPlanningMemorandumID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, auditPlanningMemorandum.NomerAPM, emailContent, urll);
            }
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(string submit, [Bind(Include = "AuditPlanningMemorandumID,NomerAPM,PreliminaryID,Date_Start,Date_End,Status,ActivityID,TujuanAudit,RuangLingkupAudit,MetodologiAudit,DataDanDokumen,AttachmentMasterID,EntryMeetingDateStart,EntryMeetingDateEnd,WalktroughDateStart,WalktroughDateEnd,FieldWorkDateStart,FieldWorkDateEnd,ExitMeetingDateStart,ExitMeetingDateEnd,LHADateStart,LHADateEnd,PICID,SupervisorID,TeamLeaderID,MemberID,ReviewMasterID,Kesimpulan,ActualEngagement")] AuditPlanningMemorandum auditPlanningMemorandum, IEnumerable<HttpPostedFileBase> files)
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
                    bool getFiles = filesTransact.getFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(auditPlanningMemorandum.NomerAPM.Replace("/", ""), i, file, server);
                    }
                }
                Session.Remove("Images");

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    auditPlanningMemorandum.Status = "Draft";
                else if (submit == "Send Back")
                    auditPlanningMemorandum.Status = HelperController.GetStatusSendback(db, "Audit Planning Memorandum", auditPlanningMemorandum.Status);
                else if (submit == "Approve")
                    auditPlanningMemorandum.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    auditPlanningMemorandum.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    auditPlanningMemorandum.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = auditPlanningMemorandum.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = auditPlanningMemorandum.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = auditPlanningMemorandum.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = auditPlanningMemorandum.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailAPM(userToSentEmail, auditPlanningMemorandum);
                        else
                            sentEmailAPM(auditPlanningMemorandum, user.Trim());
                    }
                }

                db.Entry(auditPlanningMemorandum).State = EntityState.Modified;
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                AuditPlanningMemorandum oldData = db.AuditPlanningMemorandums.AsNoTracking().Where(p => p.AuditPlanningMemorandumID.Equals(auditPlanningMemorandum.AuditPlanningMemorandumID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", auditPlanningMemorandum.AuditPlanningMemorandumID, "AuditPlanningMemorandum", oldData, auditPlanningMemorandum, username);
                db.SaveChanges();
                TempData["message"] = "Audit Planning Memorandum successfully updated!";
                return RedirectToAction("Index");
            }
            return View(auditPlanningMemorandum);
        }

        // GET: AuditPlanningMemorandums/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuditPlanningMemorandum auditPlanningMemorandum =  db.AuditPlanningMemorandums.Find(id);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            bool getFiles = filesTransact.getFiles(auditPlanningMemorandum.NomerAPM.Replace("/", ""), out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;
            if (auditPlanningMemorandum == null)
            {
                return HttpNotFound();
            }

            ViewBag.SupervisorName = auditPlanningMemorandum.SupervisorID;
            ViewBag.TeamLeaderName = auditPlanningMemorandum.TeamLeaderID;
            ViewBag.PICIDName = auditPlanningMemorandum.PICID;
            ViewBag.MemberName = auditPlanningMemorandum.MemberID;
            ViewBag.NomerPrelim = db.Preliminaries.Where(p => p.PreliminaryID.Equals(auditPlanningMemorandum.PreliminaryID)).Select(p => p.NomorPreliminarySurvey).FirstOrDefault();

            return View(auditPlanningMemorandum);
        }

        // POST: AuditPlanningMemorandums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            AuditPlanningMemorandum auditPlanningMemorandum = db.AuditPlanningMemorandums.Find(id);

            AuditPlanningMemorandum aud = new AuditPlanningMemorandum();
            db.AuditPlanningMemorandums.Remove(auditPlanningMemorandum);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "AuditPlanningMemorandum", auditPlanningMemorandum, aud, username);
            TempData["message"] = "Audit Planning Memorandum successfully deleted!";
            return RedirectToAction("Index");
        }

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
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

        public ActionResult AutocompletePre(string term)
        {
            var items = db.Preliminaries.Where(p => p.Status.Equals("Approve")).Select(p => p.NomorPreliminarySurvey).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPrelim(string nopre)
        {
            //List<Preliminary> obj = new List<Preliminary>();
            //obj = db.Preliminaries.Where(m => m.NomorPreliminarySurvey == nopre).ToList();
            //SelectList obg = new SelectList(obj, "ActivityID", "ActivityID", 0);
            PreliminaryServices no = new PreliminaryServices();
            var noap = no.GetPreliminary().Where(p => p.NomorPreliminarySurvey == nopre);

            IEnumerable<SelectListItem> apm = noap
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Activity.Name

            });
            return Json(apm);
        }

        public ActionResult GetEngPrelim(string nopre)
        {
            PreliminaryServices no = new PreliminaryServices();
            var noap = no.GetPreliminary().Where(p => p.NomorPreliminarySurvey == nopre);

            IEnumerable<SelectListItem> apm = noap
            .Select(d => new SelectListItem
            {
                Value = d.EngagementID.ToString(),
                Text = d.EngagementActivity.Name

            });
            return Json(apm);
        }

        [HttpPost]
        public string GetMember(string nopre)
        {
            Preliminary pre = db.Preliminaries.Where( p => p.NomorPreliminarySurvey.Equals(nopre)).FirstOrDefault();
            if (pre != null)
            {
                var memberId = pre.MemberID;
                var teamleaderId = pre.TeamLeaderID;
                var supervisorId = pre.SupervisorID;
                var picId = pre.PICID;
                var activityid = pre.Activity.ActivityID;
                var engagement = pre.EngagementActivity.Name;
                string memberName = memberId;
                string activity = db.Activities.Find(activityid).Name;
                Walktrough wt = db.Walktroughs.Where(p => p.PreliminaryID == pre.PreliminaryID).FirstOrDefault();
                string wlkStartDate = wt.Date_Start.ToShortDateString();
                string wlkEndDate = wt.Date_End.ToShortDateString();
                if (teamleaderId == "" || supervisorId == "" || picId == "")
                {
                    //string memberName = "";
                    string teamleaderName = "Null";
                    string supervisorName = "Null";
                    string picName = "Null";
                    string memberName1 = "Null";
                    string names = teamleaderName + "," + supervisorName + "," + picName;
                    return names + "," + memberName1 + "," + activity + "," + engagement + ',' + wlkStartDate + ',' + wlkEndDate;
                }
                else
                {
                    //string memberName = db.Employees.Find(memberId).Name;
                    //string teamleaderName = db.Employees.Find(teamleaderId).Name;
                    //string supervisorName = db.Employees.Find(supervisorId).Name;
                    //string picName = db.Employees.Find(picId).Name;
                    string teamleaderName = teamleaderId;
                    string supervisorName = supervisorId;
                    string picName = picId;
                    string names = teamleaderName + "," + supervisorName + "," + picName;
                    return names + "," + memberName + "," + activity + "," + engagement + ',' + wlkStartDate + ',' + wlkEndDate;
                }
            }
            else return ",,,,,,";
        }

        [HttpPost]
        public ActionResult UpdatePersonil(int ideng, string pic, string supervisor, string leader,string member,int id)
        {
            if (ModelState.IsValid)
            {
                var eng = db.EngagementActivities.Find(ideng);
                eng.PICID = pic;
                eng.SupervisorID = supervisor;
                eng.TeamLeaderID = leader;
                eng.MemberID = member;
                db.Entry(eng).State = EntityState.Modified;
                var audit = db.AuditPlanningMemorandums.Find(id);
                audit.PICID = pic;
                audit.SupervisorID = supervisor;
                audit.TeamLeaderID = leader;
                audit.MemberID = member;
                db.Entry(audit).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Audit Planning Memorandum successfully updated!";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuditPlanningMemorandum AuditPlanningMemorandum = await db.AuditPlanningMemorandums.FindAsync(id);

            ViewBag.WordDocumentFilename = "APM";
            return View(AuditPlanningMemorandum);
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

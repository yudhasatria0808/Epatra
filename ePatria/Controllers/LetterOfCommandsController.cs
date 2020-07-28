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
   [Authorize]
    public class LetterOfCommandsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private HelperController helperTransact = new HelperController();

        // GET: LetterOfCommands
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var letterOfCommands = db.LetterOfCommands.Include(l => l.AuditPlanningMemorandum);
            return View(await letterOfCommands.ToListAsync());
        }

        // GET: LetterOfCommands/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LetterOfCommand letterOfCommand = await db.LetterOfCommands.FindAsync(id);
            string letterOfCommandDetailDasar = db.LetterOfCommandDetailDasars.Find(id).Dasar;
            string letterOfCommandDetailTembusan = db.LetterOfCommandDetailTembusans.Find(id).Tembusan;
            string letterOfCommandDetailUntuk = db.LetterOfCommandDetailUntuks.Find(id).Untuk;
            ViewBag.Dasar = letterOfCommandDetailDasar;
            ViewBag.Untuk = letterOfCommandDetailUntuk;
            ViewBag.Tembusan = letterOfCommandDetailTembusan;
            ViewBag.NomerAPM = db.AuditPlanningMemorandums.Find(letterOfCommand.AuditPlanningMemorandumID).NomerAPM;
            //ViewBag.MemberName = db.Employees.Where(p => p.EmployeeID.Equals(letterOfCommand.MemberID)).Select(p => p.Name).FirstOrDefault();
            //ViewBag.SupervisorName = db.Employees.Where(p => p.EmployeeID.Equals(letterOfCommand.SupervisorID)).Select(p => p.Name).FirstOrDefault();
            //ViewBag.TeamLeaderName = db.Employees.Where(p => p.EmployeeID.Equals(letterOfCommand.TeamLeaderID)).Select(p => p.Name).FirstOrDefault();
            //ViewBag.PICIDName = db.Employees.Where(p => p.EmployeeID.Equals(letterOfCommand.PICID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.MemberName = letterOfCommand.MemberID;
            ViewBag.SupervisorName = letterOfCommand.SupervisorID;
            ViewBag.TeamLeaderName = letterOfCommand.TeamLeaderID;
            ViewBag.PICIDName = letterOfCommand.PICID;
            ViewBag.member = letterOfCommand.MemberID;
           
            if (letterOfCommand == null)
            {
                return HttpNotFound();
            }
            return View(letterOfCommand);
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LetterOfCommand letterOfCommand = await db.LetterOfCommands.FindAsync(id);
            string letterOfCommandDetailDasar = db.LetterOfCommandDetailDasars.Find(id).Dasar;
            string letterOfCommandDetailTembusan = db.LetterOfCommandDetailTembusans.Find(id).Tembusan;
            string letterOfCommandDetailUntuk = db.LetterOfCommandDetailUntuks.Find(id).Untuk;
            ViewBag.Dasar = letterOfCommandDetailDasar;
            ViewBag.Untuk = letterOfCommandDetailUntuk;
            ViewBag.Tembusan = letterOfCommandDetailTembusan;
            ViewBag.MemberName = letterOfCommand.MemberID;
            ViewBag.SupervisorName = letterOfCommand.SupervisorID;
            ViewBag.TeamLeaderName = letterOfCommand.TeamLeaderID;
            ViewBag.PICIDName = letterOfCommand.PICID;


            List<LetterOfCommandDetailUntuk> untukList = new List<LetterOfCommandDetailUntuk>();
            List<LetterOfCommandDetailUntuk> Untuks = new List<LetterOfCommandDetailUntuk>();
            List<LetterOfCommandDetailTembusan> tembusanList = new List<LetterOfCommandDetailTembusan>();
            List<LetterOfCommandDetailTembusan> Tembusans = new List<LetterOfCommandDetailTembusan>();

            Untuks = db.LetterOfCommandDetailUntuks.Where(p => p.LetterOfCommandID.Equals(letterOfCommand.LetterOfCommandID)).ToList();
            foreach (var u in Untuks)
            {
                untukList.Add(u);
            }

            Tembusans = db.LetterOfCommandDetailTembusans.Where(p => p.LetterOfCommandID.Equals(letterOfCommand.LetterOfCommandID)).ToList();
            foreach (var u in Tembusans)
            {
                tembusanList.Add(u);
            }

            if (letterOfCommand == null)
            {
                return HttpNotFound();
            }

            ViewBag.Untuks = untukList;
            ViewBag.Tembusans = tembusanList;

            ViewBag.WordDocumentFilename = "SuratPerintah";
            return View(letterOfCommand);
        }

        // GET: LetterOfCommands/Create
        public ActionResult Create()
        {
            string spNo = helperTransact.getNewNumber("SP");
            ViewBag.nomersp = spNo;
            return View();
        }

        // POST: LetterOfCommands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "LetterOfCommandID,NomorSP,Status,Remarks,AssuranceType,Date_Start,Date_End,Menimbang,Penutup,AuditPlanningMemorandumID,ActivityID,PICID,SupervisorID,TeamLeaderID,MemberID")] LetterOfCommand letterOfCommand, 
            LetterOfCommandDetailDasar letterOfCommandDetailDasar, LetterOfCommandDetailUntuk letterOfCommandDetailUntuk,
            LetterOfCommandDetailTembusan letterOfCommandDetailTembusan, string letterno, string membernull, 
            string activid, string[] members, string pic, string supervisor, string leader, string nomer, 
            string submit, string[] dasar, string[] untuk, string[] tembusan)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;

                HttpServerUtilityBase server = Server;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    letterOfCommand.Status = "Draft";
                else if (submit == "Send Back")
                    letterOfCommand.Status = HelperController.GetStatusSendback(db, "Surat Perintah", letterOfCommand.Status);
                else if (submit == "Approve")
                    letterOfCommand.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    letterOfCommand.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    letterOfCommand.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = letterOfCommand.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = letterOfCommand.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = letterOfCommand.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = letterOfCommand.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                }

                IQueryable<AuditPlanningMemorandum> apm = db.AuditPlanningMemorandums.Where(p => p.NomerAPM.Equals(nomer));
                int apmId = 0;
                if (apm.Count() > 0 || apm != null)
                {
                    apmId = apm.Select(p => p.AuditPlanningMemorandumID).FirstOrDefault();
                    letterOfCommand.AuditPlanningMemorandumID = apmId;
                }
                letterOfCommand.SupervisorID = supervisor;
                letterOfCommand.TeamLeaderID = leader;
                letterOfCommand.PICID = pic;


                IQueryable<Activity> act = db.Activities.Where(p => p.Name.Equals(activid));
                int activ = 0;
                if (act.Count() > 0 || act != null)
                    activ = act.Select(p => p.ActivityID).FirstOrDefault();
                letterOfCommand.ActivityID = activ;
           
                string tahun1 = letterOfCommand.Date_Start.ToShortDateString().Split('/')[2] + ";";
                string tahun2 = letterOfCommand.Date_End.ToShortDateString().Split('/')[2] + ";";
                Activity actLetter = db.Activities.Find(activ);
                if (actLetter.Tahun != null)
                {
                    if (tahun1 != tahun2)
                    {
                        if (!actLetter.Tahun.Contains(tahun1))
                            actLetter.Tahun += tahun1;
                        if (!actLetter.Tahun.Contains(tahun2))
                            actLetter.Tahun += tahun2;
                    }
                    else
                    {
                        if (!actLetter.Tahun.Contains(tahun1))
                            actLetter.Tahun += tahun1;
                    }
                } else
                {
                    if (tahun1 != tahun2)
                        actLetter.Tahun = tahun1 + tahun2;
                    else actLetter.Tahun = tahun1;

                }
                db.Entry(actLetter).State = EntityState.Modified;
                letterOfCommand.NomorSP = letterno;
                foreach (var mb in members)
                {
                    letterOfCommand.MemberID += mb + ";";
                }
                letterOfCommandDetailDasar.LetterOfCommandID = letterOfCommand.LetterOfCommandID;
                letterOfCommandDetailDasar.Dasar = null;
                foreach (var ds in dasar)
                {
                    letterOfCommandDetailDasar.Dasar += ds + ";";
                }
                letterOfCommandDetailUntuk.LetterOfCommandID = letterOfCommand.LetterOfCommandID;
                letterOfCommandDetailUntuk.Untuk = null;
                foreach (var utk in untuk)
                {
                    letterOfCommandDetailUntuk.Untuk += utk + ";";
                }
                letterOfCommandDetailTembusan.LetterOfCommandID = letterOfCommand.LetterOfCommandID;
                letterOfCommandDetailTembusan.Tembusan = null;
                foreach (var tmb in tembusan)
                {
                    letterOfCommandDetailTembusan.Tembusan += tmb + ";";
                }
                db.LetterOfCommandDetailDasars.Add(letterOfCommandDetailDasar);
                db.LetterOfCommandDetailTembusans.Add(letterOfCommandDetailTembusan);
                db.LetterOfCommandDetailUntuks.Add(letterOfCommandDetailUntuk);
                db.LetterOfCommands.Add(letterOfCommand);
                db.SaveChanges();
                LetterOfCommand let = new LetterOfCommand();
                LetterOfCommandDetailDasar letDasar = new LetterOfCommandDetailDasar();
                LetterOfCommandDetailUntuk letUntuk = new LetterOfCommandDetailUntuk();
                LetterOfCommandDetailTembusan letTembusan = new LetterOfCommandDetailTembusan();
                auditTransact.CreateAuditTrail("Create", letterOfCommand.LetterOfCommandID, "LetterOfCommand", let, letterOfCommand, username);
                auditTransact.CreateAuditTrail("Create", letterOfCommandDetailDasar.ID, "LetterOfCommandDetailDasar", letDasar, letterOfCommandDetailDasar, username);
                auditTransact.CreateAuditTrail("Create", letterOfCommandDetailUntuk.ID, "LetterOfCommandDetailUntuk", letUntuk, letterOfCommandDetailUntuk, username);
                auditTransact.CreateAuditTrail("Create", letterOfCommandDetailTembusan.ID, "LetterOfCommandDetailTembusan", letTembusan, letterOfCommandDetailTembusan, username);
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "letter";
                rrm.Description = page + letterOfCommand.LetterOfCommandID;
                
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();
                TempData["message"] = "Letter Of Command successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.PreliminaryID = new SelectList(db.Preliminaries, "PreliminaryID", "NomorPreliminarySurvey", letterOfCommand.PreliminaryID);
            return View(letterOfCommand);
        }

        // GET: LetterOfCommands/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LetterOfCommand letterOfCommand = db.LetterOfCommands.Find(id);
            string letterOfCommandDetailDasar = db.LetterOfCommandDetailDasars.Find(id).Dasar;
            string letterOfCommandDetailTembusan = db.LetterOfCommandDetailTembusans.Find(id).Tembusan;
            string letterOfCommandDetailUntuk = db.LetterOfCommandDetailUntuks.Find(id).Untuk;
            ViewBag.Dasar = letterOfCommandDetailDasar;
            ViewBag.Untuk = letterOfCommandDetailUntuk;            
            ViewBag.Tembusan = letterOfCommandDetailTembusan;

            if (letterOfCommand == null)
            {
                return HttpNotFound();
            }

            PreliminaryServices activity = new PreliminaryServices();
            var activities = activity.GetPreliminary().Where(p => p.PreliminaryID == letterOfCommand.AuditPlanningMemorandum.PreliminaryID);

            ViewBag.activid = db.Activities.Where(p => p.ActivityID.Equals(letterOfCommand.ActivityID)).Select(p => p.Name).FirstOrDefault();
            string eng = db.EngagementActivities.Find(letterOfCommand.AuditPlanningMemorandum.Preliminary.EngagementID).Name;
            ViewBag.enggname = eng;
            ViewBag.nomer = db.AuditPlanningMemorandums.Find(letterOfCommand.AuditPlanningMemorandumID).NomerAPM;
            ViewBag.member = letterOfCommand.MemberID;
            ViewBag.supervisor = letterOfCommand.SupervisorID;
            ViewBag.leader = letterOfCommand.TeamLeaderID;
            ViewBag.pic = letterOfCommand.PICID;
           
            return View(letterOfCommand);
        }

        // POST: LetterOfCommands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string submit, string Status, string[] Dasar, string[] Untuk, string[] Tembusan, string activid, string memberName, string[] member, string pic, string supervisor, string leader, string nomer, [Bind(Include = "LetterOfCommandID,NomorSP,Status,Remarks,AssuranceType,Date_Start,Date_End,Menimbang,Penutup,AuditPlanningMemorandumID,ActivityID,PICID,SupervisorID,TeamLeaderID,MemberID")] LetterOfCommand letterOfCommand)
        {
            if (ModelState.IsValid)
            {
                letterOfCommand.Status = Status;

                IQueryable<AuditPlanningMemorandum> apm = db.AuditPlanningMemorandums.Where(p => p.NomerAPM.Equals(nomer));
                int apmId = 0;
                if (apm.Count() > 0 || apm != null)
                {
                    apmId = apm.Select(p => p.AuditPlanningMemorandumID).FirstOrDefault();
                    letterOfCommand.AuditPlanningMemorandumID = apmId;
                }
                letterOfCommand.SupervisorID = supervisor;
                letterOfCommand.TeamLeaderID = leader;
                letterOfCommand.PICID = pic;
                if (member != null)
                {
                    foreach (var mb in member)
                    {
                        letterOfCommand.MemberID += mb + ";";
                    }
                }
                else
                    letterOfCommand.MemberID += ";";
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    letterOfCommand.Status = "Draft";
                else if (submit == "Send Back")
                    letterOfCommand.Status = HelperController.GetStatusSendback(db, "Surat Perintah", letterOfCommand.Status);
                else if (submit == "Approve")
                    letterOfCommand.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    letterOfCommand.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    letterOfCommand.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = letterOfCommand.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = letterOfCommand.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = letterOfCommand.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = letterOfCommand.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                }

                IQueryable<Activity> act = db.Activities.Where(p => p.Name.Equals(activid));
                int activ = 0;
                if (act.Count() > 0 || act != null)
                    activ = act.Select(p => p.ActivityID).FirstOrDefault();
                letterOfCommand.ActivityID = activ;

                db.Configuration.ProxyCreationEnabled = false;
                LetterOfCommandDetailDasar letterOfCommandDetailDasar = db.LetterOfCommandDetailDasars.AsNoTracking().Where(p => p.LetterOfCommandID == letterOfCommand.LetterOfCommandID).FirstOrDefault();
                LetterOfCommandDetailUntuk letterOfCommandDetailUntuk = db.LetterOfCommandDetailUntuks.AsNoTracking().Where(p => p.LetterOfCommandID == letterOfCommand.LetterOfCommandID).FirstOrDefault();
                LetterOfCommandDetailTembusan letterOfCommandDetailTembusan = db.LetterOfCommandDetailTembusans.AsNoTracking().Where(p => p.LetterOfCommandID == letterOfCommand.LetterOfCommandID).FirstOrDefault();
                letterOfCommandDetailDasar.LetterOfCommandID = letterOfCommand.LetterOfCommandID;
                letterOfCommandDetailDasar.Dasar = null;
                if (Dasar != null)
                {
                    foreach (var ds in Dasar)
                    {
                        letterOfCommandDetailDasar.Dasar += ds + ";";
                    }
                }
                else
                    letterOfCommandDetailDasar.Dasar = ";";

                letterOfCommandDetailUntuk.LetterOfCommandID = letterOfCommand.LetterOfCommandID;
                letterOfCommandDetailUntuk.Untuk = null;
                if (Untuk != null)
                {
                    foreach (var uk in Untuk)
                    {
                        letterOfCommandDetailUntuk.Untuk += uk + ";";
                    }
                }
                else
                    letterOfCommandDetailUntuk.Untuk = ";";

                letterOfCommandDetailTembusan.LetterOfCommandID = letterOfCommand.LetterOfCommandID;
                letterOfCommandDetailTembusan.Tembusan = null;
                if (Tembusan != null)
                {
                    foreach (var ts in Tembusan)
                    {
                    letterOfCommandDetailTembusan.Tembusan += ts + ";";
                    }
                }
                else
                    letterOfCommandDetailTembusan.Tembusan = ";";

                string username = User.Identity.Name;
                LetterOfCommand oldData = db.LetterOfCommands.AsNoTracking().Where(p => p.LetterOfCommandID.Equals(letterOfCommand.LetterOfCommandID)).FirstOrDefault();
                LetterOfCommandDetailDasar oldDataDasar = db.LetterOfCommandDetailDasars.AsNoTracking().Where(p => p.ID.Equals(letterOfCommandDetailDasar.ID)).FirstOrDefault();
                LetterOfCommandDetailUntuk oldDataUntuk = db.LetterOfCommandDetailUntuks.AsNoTracking().Where(p => p.ID.Equals(letterOfCommandDetailUntuk.ID)).FirstOrDefault();
                LetterOfCommandDetailTembusan oldDataTembusan = db.LetterOfCommandDetailTembusans.AsNoTracking().Where(p => p.ID.Equals(letterOfCommandDetailTembusan.ID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", letterOfCommand.LetterOfCommandID, "LetterOfCommand", oldData, letterOfCommand, username);
                auditTransact.CreateAuditTrail("Update", letterOfCommandDetailDasar.ID, "LetterOfCommandDasar", oldDataDasar, letterOfCommandDetailDasar, username);
                auditTransact.CreateAuditTrail("Update", letterOfCommandDetailUntuk.ID, "LetterOfCommandUntuk", oldDataUntuk, letterOfCommandDetailUntuk, username);
                auditTransact.CreateAuditTrail("Update", letterOfCommandDetailTembusan.ID, "LetterOfCommandTembusan", oldDataTembusan, letterOfCommandDetailTembusan, username);

                db.Entry(letterOfCommandDetailDasar).State = EntityState.Modified;
                db.Entry(letterOfCommandDetailUntuk).State = EntityState.Modified;
                db.Entry(letterOfCommandDetailTembusan).State = EntityState.Modified;
                db.Entry(letterOfCommand).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["message"] = "Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.PreliminaryID = new SelectList(db.Preliminaries, "PreliminaryID", "Type", letterOfCommand.PreliminaryID);
            return View(letterOfCommand);
        }

        private bool sentEmailSP(LetterOfCommand letterOfCommand, string user)
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
                        string emailContent = "Dear {0}, <BR/><BR/>Letter of Command : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Letter of Command\">link</a> to show the Letter of Command.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/LetterOfCommands/Details/" + letterOfCommand.LetterOfCommandID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, letterOfCommand.NomorSP, emailContent, url);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailSP(string userToSentEmail, LetterOfCommand letterOfCommand)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>Letter of Command : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Letter of Command\">link</a> to show the Letter of Command.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = baseUrl + "/LetterOfCommands/Details/" + letterOfCommand.LetterOfCommandID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, letterOfCommand.NomorSP, emailContent, url);
            }
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(string submit, [Bind(Include = "LetterOfCommandID,NomorSP,Status,Remarks,AssuranceType,Date_Start,Date_End,Menimbang,Penutup,AuditPlanningMemorandumID,ActivityID,PICID,SupervisorID,TeamLeaderID,MemberID")] LetterOfCommand letterOfCommand)
        {
            if (ModelState.IsValid)
            {

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    letterOfCommand.Status = "Draft";
                else if (submit == "Send Back")
                    letterOfCommand.Status = HelperController.GetStatusSendback(db, "Surat Perintah", letterOfCommand.Status);
                else if (submit == "Approve")
                    letterOfCommand.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    letterOfCommand.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    letterOfCommand.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = letterOfCommand.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = letterOfCommand.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = letterOfCommand.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = letterOfCommand.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailSP(userToSentEmail, letterOfCommand);
                        else
                            sentEmailSP(letterOfCommand, user.Trim());
                    }
                }
                
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                LetterOfCommand oldData = db.LetterOfCommands.AsNoTracking().Where(p => p.LetterOfCommandID.Equals(letterOfCommand.LetterOfCommandID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", letterOfCommand.LetterOfCommandID, "LetterOfCommand", oldData, letterOfCommand, username);
                db.Entry(letterOfCommand).State = EntityState.Modified;
                await db.SaveChangesAsync();

                TempData["message"] = "Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.PreliminaryID = new SelectList(db.Preliminaries, "PreliminaryID", "Type", letterOfCommand.PreliminaryID);
            return View(letterOfCommand);
        }


        [HttpPost]
        public ActionResult UpdatePersonil(int ideng, string pic, string supervisor, string leader, string member, int id)
        {
            if (ModelState.IsValid)
            {
                var eng = db.EngagementActivities.Find(ideng);
                eng.PICID = pic;
                eng.SupervisorID = supervisor;
                eng.TeamLeaderID = leader;
                eng.MemberID = member;
                db.Entry(eng).State = EntityState.Modified;
                var letter = db.LetterOfCommands.Find(id);
                letter.PICID = pic;
                letter.SupervisorID = supervisor;
                letter.TeamLeaderID = leader;
                letter.MemberID = member;
                db.Entry(letter).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        //POST : Update Dasar
        [HttpPost]
        public ActionResult UpdateDasar(string dasar, int id)
        {
            if (ModelState.IsValid)
            {

                var iddasar = db.LetterOfCommandDetailDasars.Where(p => p.LetterOfCommandID.Equals(id)).Select(p => p.ID).FirstOrDefault();
                var Ldasar = db.LetterOfCommandDetailDasars.Find(iddasar);
                Ldasar.Dasar = dasar;
                db.Entry(Ldasar).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        //POST : Update Untuk
        [HttpPost]
        public ActionResult UpdateUntuk(string untuk, int id)
        {
            if (ModelState.IsValid)
            {

                var iduntuk = db.LetterOfCommandDetailUntuks.Where(p => p.LetterOfCommandID.Equals(id)).Select(p => p.ID).FirstOrDefault();
                var Luntuk = db.LetterOfCommandDetailUntuks.Find(iduntuk);
                Luntuk.Untuk = untuk;
                db.Entry(Luntuk).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        //POST : Update Tembusan
        [HttpPost]
        public ActionResult UpdateTembusan(string tembusan, int id)
        {
            if (ModelState.IsValid)
            {

                var idtembusan = db.LetterOfCommandDetailTembusans.Where(p => p.LetterOfCommandID.Equals(id)).Select(p => p.ID).FirstOrDefault();
                var Ltembusan = db.LetterOfCommandDetailTembusans.Find(idtembusan);
                Ltembusan.Tembusan = tembusan;
                db.Entry(Ltembusan).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }


        // GET: LetterOfCommands/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LetterOfCommand letterOfCommand = await db.LetterOfCommands.FindAsync(id);
            string letterOfCommandDetailDasar = db.LetterOfCommandDetailDasars.Find(id).Dasar;
            string letterOfCommandDetailTembusan = db.LetterOfCommandDetailTembusans.Find(id).Tembusan;
            string letterOfCommandDetailUntuk = db.LetterOfCommandDetailUntuks.Find(id).Untuk;
            ViewBag.Dasar = letterOfCommandDetailDasar;
            ViewBag.Untuk = letterOfCommandDetailUntuk;
            ViewBag.Tembusan = letterOfCommandDetailTembusan;
            ViewBag.NomerAPM = db.AuditPlanningMemorandums.Find(letterOfCommand.AuditPlanningMemorandumID).NomerAPM;
            ViewBag.MemberName = letterOfCommand.MemberID;
            ViewBag.SupervisorName = letterOfCommand.SupervisorID;
            ViewBag.TeamLeaderName = letterOfCommand.TeamLeaderID;
            ViewBag.PICIDName = letterOfCommand.PICID;
           
            if (letterOfCommand == null)
            {
                return HttpNotFound();
            }
            return View(letterOfCommand);
        }

        // POST: LetterOfCommands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            LetterOfCommand letterOfCommand = await db.LetterOfCommands.FindAsync(id);
            LetterOfCommandDetailDasar letterOfCommandDetailDasar = await db.LetterOfCommandDetailDasars.FindAsync(id);
            LetterOfCommandDetailTembusan letterOfCommandDetailTembusan = await db.LetterOfCommandDetailTembusans.FindAsync(id);
            LetterOfCommandDetailUntuk letterOfCommandDetailUntuk = await db.LetterOfCommandDetailUntuks.FindAsync(id);
            LetterOfCommand let = new LetterOfCommand();
            LetterOfCommandDetailDasar letdasar = new LetterOfCommandDetailDasar();
            LetterOfCommandDetailUntuk letuntuk = new LetterOfCommandDetailUntuk();
            LetterOfCommandDetailTembusan lettembusan = new LetterOfCommandDetailTembusan();
            db.LetterOfCommands.Remove(letterOfCommand);
            db.LetterOfCommandDetailDasars.Remove(letterOfCommandDetailDasar);
            db.LetterOfCommandDetailTembusans.Remove(letterOfCommandDetailTembusan);
            db.LetterOfCommandDetailUntuks.Remove(letterOfCommandDetailUntuk);
            await db.SaveChangesAsync();
            auditTransact.CreateAuditTrail("Delete", id, "LetterOfCommand", letterOfCommand, let, username);
            auditTransact.CreateAuditTrail("Delete", id, "LetterOfCommandDasar", letterOfCommandDetailDasar, letdasar, username);
            auditTransact.CreateAuditTrail("Delete", id, "LetterOfCommandUntuk", letterOfCommandDetailUntuk, letuntuk, username);
            auditTransact.CreateAuditTrail("Delete", id, "LetterOfCommandTembusan", letterOfCommandDetailTembusan, lettembusan, username);
            TempData["message"] = "Letter Of Command successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult AutocompletePre(string term)
        {
            var items = db.NotulenEntryMeetings.Select(p => p.AuditPlanningMemorandum.Preliminary.NomorPreliminarySurvey).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AutocompleteAPM(string term)
        {
            var items = db.AuditPlanningMemorandums.Where(p => p.Status.Equals("Approve")).Select(p => p.NomerAPM).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public string GetMember(string noapm)
        {
            AuditPlanningMemorandum apm = db.AuditPlanningMemorandums.Where(p => p.NomerAPM.Equals(noapm)).FirstOrDefault();
            if (apm != null)
            {
                Preliminary pre = apm.Preliminary;
                //var memberId = pre.EngagementActivity.MemberID;
                //var teamleaderId = pre.EngagementActivity.TeamLeaderID;
                //var supervisorId = pre.EngagementActivity.SupervisorID;
                //var picId = pre.EngagementActivity.PICID;
                var memberId = apm.MemberID;
                var teamleaderId = apm.TeamLeaderID;
                var supervisorId = apm.SupervisorID;
                var picId = apm.PICID;
                string memberName = memberId;
                string nomerapm = apm.NomerAPM;
                //string engagement = pre.EngagementActivity.Name;
                string engagement = "";
                if (apm.ActualEngagement != "" && apm.ActualEngagement != null)
                {
                    engagement = apm.ActualEngagement;
                }
                else
                {
                    engagement = pre.EngagementActivity.Name;
                }
                string activity = pre.Activity.Name;
                string apmStartDate = apm.Date_Start.ToShortDateString();
                string apmEndDate = apm.Date_End.ToShortDateString();
                if (teamleaderId == "" || supervisorId == "" || picId == "")
                {
                    //string memberName = "";
                    string teamleaderName = "";
                    string supervisorName = "";
                    string picName = "";
                    string names = teamleaderName + "," + supervisorName + "," + picName;
                    return names + "," + memberName + "," + nomerapm + "," + engagement + "," + activity + ',' + apmStartDate + ',' + apmEndDate;
                }
                else
                {
                    //string memberName = db.Employees.Find(memberId).Name;
                    string teamleaderName = teamleaderId;
                    string supervisorName = supervisorId;
                    string picName = picId;
                    string names = teamleaderName + "," + supervisorName + "," + picName;
                    return names + "," + memberName + "," + nomerapm + "," + engagement + "," + activity + ',' + apmStartDate + ',' + apmEndDate;
                }
            }
            else return ",,,,,,";
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

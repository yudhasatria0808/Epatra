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
    public class ConsultingLetterOfCommandsController : Controller
    {
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private ePatriaDefault context = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private HelperController helperTransact = new HelperController();
        //
        // GET: /ConsultingLetterOfCommandModels/

        public ViewResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            return View(context.ConsultingLetterOfCommands.ToList());
        }

        //
        // GET: /ConsultingLetterOfCommandModels/Details/5

        public ViewResult Details(int id)
        {
            ConsultingLetterOfCommand consultingletterofcommand = context.ConsultingLetterOfCommands.Single(x => x.ConsultingSuratPerintahID == id);
            ViewBag.ActivityName = context.EngagementActivities.Where(p => p.Name.Equals(consultingletterofcommand.EngagementName)).Select(p => p.ActivityStr).FirstOrDefault();
            ViewBag.Dasar = context.ConsultingLetterOfCommandDetailDasars.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Dasar).FirstOrDefault();
            ViewBag.Untuk = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Untuk).FirstOrDefault();
            ViewBag.Tembusan = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Tembusan).FirstOrDefault();
            ViewBag.member = consultingletterofcommand.MemberID;
          
            return View(consultingletterofcommand);
        }

        //
        // GET: /ConsultingLetterOfCommandModels/Create

        public ActionResult Create()
        {
            string spNo = helperTransact.getNewNumber("CSP");
            ViewBag.NomorSP = spNo;
            return View();
        } 

        //
        // POST: /ConsultingLetterOfCommandModels/Create

        [HttpPost]
        public ActionResult Create([Bind(Include = "ConsultingSuratPerintahID,NomorSP,ConsultingRequestID,EngagementName,EngagementID,StartDate,EndDate,PicID,SupervisorID,TeamLeaderID,Menimbang,Penutup,MemberID,Remarks")] ConsultingLetterOfCommand consultingletterofcommand, 
            EngagementActivity engagementActivity, string ActivityName, string submit,
            string[] member, string ConsultingRequest, ConsultingLetterOfCommandDetailDasar Ldasar, 
            ConsultingLetterOfCommandDetailTembusan Ltembusan, ConsultingLetterOfCommandDetailUntuk Luntuk,
            string[] dasar, string[] tembusan, string[] untuk)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                IQueryable<ConsultingRequest> cr = context.ConsultingRequests.Where(p => p.NoRequest.Equals(ConsultingRequest));
                int ConsulId = 0;
                if (cr.Count() > 0 || cr != null)
                    ConsulId = cr.Select(p => p.ConsultingRequestID).FirstOrDefault();
                consultingletterofcommand.ConsultingRequestID = ConsulId;
                foreach (var mb in member)
                {
                    consultingletterofcommand.MemberID += mb + ";";
                }
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    consultingletterofcommand.Status = "Draft";
                else if (submit == "Send Back")
                    consultingletterofcommand.Status = HelperController.GetStatusSendback(context, "Consulting Surat Perintah", consultingletterofcommand.Status);
                else if (submit == "Approve")
                    consultingletterofcommand.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    consultingletterofcommand.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                    consultingletterofcommand.Status = "Pending for Approve by" + user;
              
                context.ConsultingLetterOfCommands.Add(consultingletterofcommand);
                
                //Engagement Activity

                //IQueryable<Activity> act = context.Activities.Where(p => p.Name.Equals(ActivityName));
                //int ActivityID = 0;
                //if (act.Count() > 0 || act != null)
                //    ActivityID = act.Select(p => p.ActivityID).FirstOrDefault();
                engagementActivity.ActivityStr = ActivityName;
                engagementActivity.Name = consultingletterofcommand.EngagementName;
                engagementActivity.PICID = consultingletterofcommand.PicID;
                engagementActivity.TeamLeaderID = consultingletterofcommand.TeamLeaderID;
                engagementActivity.SupervisorID = consultingletterofcommand.SupervisorID;
                engagementActivity.MemberID = consultingletterofcommand.MemberID;
                context.EngagementActivities.Add(engagementActivity);

                //Letter of Command Dasar
                Ldasar.ConsultingSuratPerintahID = consultingletterofcommand.ConsultingSuratPerintahID;
                Ldasar.Dasar = null;
                foreach (var ds in dasar)
                {
                    Ldasar.Dasar += ds + ";";
                }
                context.ConsultingLetterOfCommandDetailDasars.Add(Ldasar);

                //Letter of Command Untuk
                Luntuk.ConsultingSuratPerintahID = consultingletterofcommand.ConsultingSuratPerintahID;
                Luntuk.Untuk = null;
                foreach (var utk in untuk)
                {
                    Luntuk.Untuk += utk + ";";
                }
                context.ConsultingLetterOfCommandDetailUntuks.Add(Luntuk);

                //Letter of Command Tembusan
                Ltembusan.ConsultingSuratPerintahID = consultingletterofcommand.ConsultingSuratPerintahID;
                Ltembusan.Tembusan = null;
                foreach (var tmb in tembusan)
                {
                    Ltembusan.Tembusan += tmb + ";";
                }
                context.ConsultingLetterOfCommandDetailTembusans.Add(Ltembusan);

                context.SaveChanges();

                //assign engagementID in consulting SP
                consultingletterofcommand.EngagementID = engagementActivity.EngagementID;
                context.Entry(consultingletterofcommand).State = EntityState.Modified;

                //Review Relation Master
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "conleterdetail";
                rrm.Description = page + consultingletterofcommand.ConsultingSuratPerintahID;
                context.ReviewRelationMasters.Add(rrm);
                context.SaveChanges();
                ConsultingLetterOfCommand conletcom = new ConsultingLetterOfCommand();
                ConsultingLetterOfCommandDetailDasar conletdasar = new ConsultingLetterOfCommandDetailDasar();
                ConsultingLetterOfCommandDetailUntuk conletuntuk = new ConsultingLetterOfCommandDetailUntuk();
                ConsultingLetterOfCommandDetailTembusan conlettembusan = new ConsultingLetterOfCommandDetailTembusan();
                ConsultingLetterOfCommand conletnew = context.ConsultingLetterOfCommands.Find(consultingletterofcommand.ConsultingSuratPerintahID);
                auditTransact.CreateAuditTrail("Create", consultingletterofcommand.ConsultingSuratPerintahID, "Consulting Letter Of Command", conletcom, conletnew, username);
                auditTransact.CreateAuditTrail("Create", Ldasar.ConsultingLetterOfCommandDetailDasarID, "Consulting Letter Of Command Dasar", conletdasar, Ldasar, username);
                auditTransact.CreateAuditTrail("Create", Luntuk.ConsultingLetterOfCommandDetailUntukID, "Consulting Letter Of Command Untuk", conletuntuk, Luntuk, username);
                auditTransact.CreateAuditTrail("Create", Ltembusan.ConsultingLetterOfCommandDetailTembusanID, "Consulting Letter Of Command Tembusan", conlettembusan, Ltembusan, username);
                TempData["message"] = "Consulting Letter Of Command successfully created!";
                return RedirectToAction("Index");  
            }
         
            return View(consultingletterofcommand);
        }
        
        //
        // GET: /ConsultingLetterOfCommandModels/Edit/5

        public ActionResult Edit(int id)
        {
            ConsultingLetterOfCommand consultingletterofcommand = context.ConsultingLetterOfCommands.Single(x => x.ConsultingSuratPerintahID == id);
            ViewBag.ConsultingRequest = context.ConsultingRequests.Where(p => p.ConsultingRequestID.Equals(consultingletterofcommand.ConsultingRequestID)).Select(p => p.NoRequest).FirstOrDefault();
            ViewBag.ActivityName = context.EngagementActivities.Where(p=>p.Name.Equals(consultingletterofcommand.EngagementName)).Select(p=>p.ActivityStr).FirstOrDefault();
            ViewBag.Dasar = context.ConsultingLetterOfCommandDetailDasars.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Dasar).FirstOrDefault();
            ViewBag.Untuk = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Untuk).FirstOrDefault();
            ViewBag.Tembusan = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Tembusan).FirstOrDefault();
            ViewBag.enggname = consultingletterofcommand.EngagementName;
            ViewBag.member = consultingletterofcommand.MemberID;
            return View(consultingletterofcommand);
        }

        //
        // POST: /ConsultingLetterOfCommandModels/Edit/5

        [HttpPost]
        public ActionResult Edit([Bind(Include = "ConsultingSuratPerintahID,NomorSP,ConsultingRequestID,EngagementName,Status,StartDate,EndDate,PicID,SupervisorID,TeamLeaderID,Menimbang,Penutup,MemberID,Remarks,EngagementID")] 
            ConsultingLetterOfCommand consultingletterofcommand, string ConsultingRequest, string enggname, string submit, string ActivityName, string[] member, string[] Dasar, string[] Untuk, string[] Tembusan)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                context.Configuration.ProxyCreationEnabled = false;
                IQueryable<ConsultingRequest> cr = context.ConsultingRequests.Where(p => p.NoRequest.Equals(ConsultingRequest));
                int ConsulId = 0;
                if (cr.Count() > 0 || cr != null)
                    ConsulId = cr.Select(p => p.ConsultingRequestID).FirstOrDefault();
                consultingletterofcommand.ConsultingRequestID = ConsulId;
                if (member != null)
                {
                    foreach (var mb in member)
                    {
                        consultingletterofcommand.MemberID += mb + ";";
                    }
                }
                else
                    consultingletterofcommand.MemberID += ";";
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    consultingletterofcommand.Status = "Draft";
                else if (submit == "Send Back")
                    consultingletterofcommand.Status = HelperController.GetStatusSendback(context, "Consulting Surat Perintah", consultingletterofcommand.Status);
                else if (submit == "Approve")
                    consultingletterofcommand.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    consultingletterofcommand.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                    consultingletterofcommand.Status = "Pending for Approve by" + user;
                ConsultingLetterOfCommand oldData = context.ConsultingLetterOfCommands.AsNoTracking().Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", consultingletterofcommand.ConsultingSuratPerintahID, "Consulting Letter Of Command", oldData, consultingletterofcommand, username);
                context.Entry(consultingletterofcommand).State = EntityState.Modified;
                context.SaveChanges();

                ConsultingLetterOfCommandDetailDasar ConsultingletterOfCommandDetailDasar = context.ConsultingLetterOfCommandDetailDasars.Where(p => p.ConsultingSuratPerintahID == consultingletterofcommand.ConsultingSuratPerintahID).FirstOrDefault();
                ConsultingletterOfCommandDetailDasar.Dasar = null;
                if (Dasar != null)
                {
                    foreach (var ab in Dasar)
                    {
                        ConsultingletterOfCommandDetailDasar.Dasar += ab + ";";
                    }
                }
                else
                    ConsultingletterOfCommandDetailDasar.Dasar = ";";

                ConsultingLetterOfCommandDetailUntuk ConsultingletterOfCommandDetailUntuk = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID == consultingletterofcommand.ConsultingSuratPerintahID).FirstOrDefault();
                ConsultingletterOfCommandDetailUntuk.Untuk = null;
                if (Untuk != null)
                {
                    foreach (var cd in Untuk)
                    {
                        ConsultingletterOfCommandDetailUntuk.Untuk += cd + ";";
                    }
                }
                else
                    ConsultingletterOfCommandDetailUntuk.Untuk = ";";

                ConsultingLetterOfCommandDetailTembusan ConsultingletterOfCommandDetailTembusan = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID == consultingletterofcommand.ConsultingSuratPerintahID).FirstOrDefault();
                ConsultingletterOfCommandDetailTembusan.Tembusan = null;
                if (Tembusan != null)
                {
                    foreach (var ef in Tembusan)
                    {
                        ConsultingletterOfCommandDetailTembusan.Tembusan += ef + ";";
                    }
                }
                else
                    ConsultingletterOfCommandDetailTembusan.Tembusan = ";";

                
                var eng = context.EngagementActivities.Find(consultingletterofcommand.EngagementID);
                eng.Name = consultingletterofcommand.EngagementName;
                eng.PICID = consultingletterofcommand.PicID;
                eng.TeamLeaderID = consultingletterofcommand.TeamLeaderID;
                eng.SupervisorID = consultingletterofcommand.SupervisorID;
                eng.MemberID = consultingletterofcommand.MemberID;
                ConsultingLetterOfCommandDetailDasar oldDataDasar = context.ConsultingLetterOfCommandDetailDasars.AsNoTracking().Where(p => p.ConsultingLetterOfCommandDetailDasarID.Equals(ConsultingletterOfCommandDetailDasar.ConsultingLetterOfCommandDetailDasarID)).FirstOrDefault();
                ConsultingLetterOfCommandDetailUntuk oldDataUntuk = context.ConsultingLetterOfCommandDetailUntuks.AsNoTracking().Where(p => p.ConsultingLetterOfCommandDetailUntukID.Equals(ConsultingletterOfCommandDetailUntuk.ConsultingLetterOfCommandDetailUntukID)).FirstOrDefault();
                ConsultingLetterOfCommandDetailTembusan oldDataTembusan = context.ConsultingLetterOfCommandDetailTembusans.AsNoTracking().Where(p => p.ConsultingLetterOfCommandDetailTembusanID.Equals(ConsultingletterOfCommandDetailTembusan.ConsultingLetterOfCommandDetailTembusanID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", ConsultingletterOfCommandDetailDasar.ConsultingLetterOfCommandDetailDasarID, "Consulting Letter Of Command Dasar", oldDataDasar, ConsultingletterOfCommandDetailDasar, username);
                auditTransact.CreateAuditTrail("Update", ConsultingletterOfCommandDetailUntuk.ConsultingLetterOfCommandDetailUntukID, "Consulting Letter Of Command Untuk", oldDataUntuk, ConsultingletterOfCommandDetailUntuk, username);
                auditTransact.CreateAuditTrail("Update", ConsultingletterOfCommandDetailTembusan.ConsultingLetterOfCommandDetailTembusanID, "Consulting Letter Of Command Tembusan", oldDataTembusan, ConsultingletterOfCommandDetailTembusan, username);
                context.Entry(ConsultingletterOfCommandDetailDasar).State = EntityState.Modified;
                context.Entry(ConsultingletterOfCommandDetailUntuk).State = EntityState.Modified;
                context.Entry(ConsultingletterOfCommandDetailTembusan).State = EntityState.Modified;
                context.Entry(eng).State = EntityState.Modified;
                context.SaveChanges();
                TempData["message"] = "Consulting Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            return View(consultingletterofcommand);
        }

        //
        //POST : Update Pesonil
        [HttpPost]
        public ActionResult UpdatePersonil(string pic, string supervisor, string leader, string member, string engagementname,int id)
        {
            if (ModelState.IsValid)
            {

                var engid = context.EngagementActivities.Where(p => p.Name.Equals(engagementname)).Select(p=>p.EngagementID).FirstOrDefault();
                var eng = context.EngagementActivities.Find(engid);
                eng.PICID = pic;
                eng.SupervisorID = supervisor;
                eng.TeamLeaderID = leader;
                eng.MemberID = member;
                context.Entry(eng).State = EntityState.Modified;
                var consulting = context.ConsultingLetterOfCommands.Find(id);
                consulting.PicID = pic;
                consulting.SupervisorID = supervisor;
                consulting.TeamLeaderID = leader;
                consulting.MemberID = member;
                context.Entry(consulting).State = EntityState.Modified;
                context.SaveChanges();
                TempData["message"] = "Consulting Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Consulting Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        //POST : Update Dasar
        [HttpPost]
        public ActionResult UpdateDasar(string dasar, int id)
        {
            if (ModelState.IsValid)
            {

                var iddasar = context.ConsultingLetterOfCommandDetailDasars.Where(p => p.ConsultingSuratPerintahID.Equals(id)).Select(p => p.ConsultingLetterOfCommandDetailDasarID).FirstOrDefault();
                var Ldasar = context.ConsultingLetterOfCommandDetailDasars.Find(iddasar);
                Ldasar.Dasar = dasar;
                context.Entry(Ldasar).State = EntityState.Modified;
                context.SaveChanges();
                TempData["message"] = "Consulting Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Consulting Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        //POST : Update Untuk
        [HttpPost]
        public ActionResult UpdateUntuk(string untuk, int id)
        {
            if (ModelState.IsValid)
            {

                var iduntuk = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID.Equals(id)).Select(p => p.ConsultingLetterOfCommandDetailUntukID).FirstOrDefault();
                var Luntuk = context.ConsultingLetterOfCommandDetailUntuks.Find(iduntuk);
                Luntuk.Untuk = untuk;
                context.Entry(Luntuk).State = EntityState.Modified;
                context.SaveChanges();
                TempData["message"] = "Consulting Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Consulting Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        //POST : Update Tembusan
        [HttpPost]
        public ActionResult UpdateTembusan(string tembusan, int id)
        {
            if (ModelState.IsValid)
            {

                var idtembusan = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID.Equals(id)).Select(p => p.ConsultingLetterOfCommandDetailTembusanID).FirstOrDefault();
                var Ltembusan = context.ConsultingLetterOfCommandDetailTembusans.Find(idtembusan);
                Ltembusan.Tembusan = tembusan;
                context.Entry(Ltembusan).State = EntityState.Modified;
                context.SaveChanges();
                TempData["message"] = "Consulting Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Consulting Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        private bool sentEmailCSP(ConsultingLetterOfCommand letter, string user)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
            List<Employee> CIAEmpList = new List<Employee>();
            if (CIAUserIds.Count() > 0)
            {
                var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                foreach (var CIAUser in CIAUsers)
                {
                    Employee emp = context.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();
                    if (emp != null)
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>Consulting Letter of Command : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Consulting Letter of Command\">link</a> to show the Consulting Letter of Command.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/LetterOfCommands/Details/" + letter.ConsultingSuratPerintahID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailCSP(string userToSentEmail, ConsultingLetterOfCommand letter)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = context.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>Consulting Letter of Command : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Consulting Letter of Command\">link</a> to show the Consulting Letter of Command.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = baseUrl + "/ConsultingLetterOfCommands/Details/" + letter.ConsultingSuratPerintahID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
            }
            return true;
        }

        //
        //POST : Update Status
        [HttpPost]
        public ActionResult UpdateStatus(int id,string submit)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                context.Configuration.ProxyCreationEnabled = false;
                var letter = context.ConsultingLetterOfCommands.Find(id);
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    letter.Status = "Draft";
                else if (submit == "Send Back")
                    letter.Status = HelperController.GetStatusSendback(context, "Consulting Surat Perintah", letter.Status);
                else if (submit == "Approve")
                {
                    letter.Status = "Approve";
                    ConsultingFieldWork cFw = new ConsultingFieldWork();
                    cFw.ConsultingSuratPerintahID = id;
                    cFw.Status = "Draft";
                    context.ConsultingFieldWork.Add(cFw);
                }
                else if (submit == "Submit For Review By" + user)
                    letter.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    letter.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = letter.PicID;
                        if (userToSentEmail != null)
                            sentSingleEmailCSP(userToSentEmail, letter);
                        else
                            sentEmailCSP(letter, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = letter.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailCSP(userToSentEmail, letter);
                        else
                            sentEmailCSP(letter, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = letter.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailCSP(userToSentEmail, letter);
                        else
                            sentEmailCSP(letter, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = letter.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailCSP(userToSentEmail, letter);
                        else
                            sentEmailCSP(letter, user.Trim());
                    }

                }
                
				ConsultingLetterOfCommand oldData = context.ConsultingLetterOfCommands.AsNoTracking().Where(p => p.ConsultingSuratPerintahID.Equals(letter.ConsultingSuratPerintahID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", letter.ConsultingSuratPerintahID, "Consulting Letter Of Command", oldData, letter, username);
                context.Entry(letter).State = EntityState.Modified;
                context.SaveChanges();
                
                TempData["message"] = "Consulting Letter Of Command successfully updated!";
                return RedirectToAction("Index");
            }
            TempData["message"] = "Consulting Letter Of Command successfully updated!";
            return RedirectToAction("Index");
        }

        //
        // GET: /ConsultingLetterOfCommandModels/Delete/5
 
        public ActionResult Delete(int id)
        {
            ConsultingLetterOfCommand consultingletterofcommand = context.ConsultingLetterOfCommands.Single(x => x.ConsultingSuratPerintahID == id);
            ViewBag.ActivityName = context.EngagementActivities.Where(p => p.Name.Equals(consultingletterofcommand.EngagementName)).Select(p => p.ActivityStr).FirstOrDefault();
            ViewBag.Dasar = context.ConsultingLetterOfCommandDetailDasars.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Dasar).FirstOrDefault();
            ViewBag.Untuk = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Untuk).FirstOrDefault();
            ViewBag.Tembusan = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID.Equals(consultingletterofcommand.ConsultingSuratPerintahID)).Select(p => p.Tembusan).FirstOrDefault();
            ViewBag.member = consultingletterofcommand.MemberID;
            return View(consultingletterofcommand);
        }

        //
        // POST: /ConsultingLetterOfCommandModels/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            context.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingLetterOfCommand consultingletterofcommand = context.ConsultingLetterOfCommands.Single(x => x.ConsultingSuratPerintahID == id);
            ConsultingLetterOfCommand conletcom = new ConsultingLetterOfCommand();
            ConsultingLetterOfCommandDetailDasar conletdas = new ConsultingLetterOfCommandDetailDasar();
            ConsultingLetterOfCommandDetailUntuk conletunt = new ConsultingLetterOfCommandDetailUntuk();
            ConsultingLetterOfCommandDetailTembusan conlettem = new ConsultingLetterOfCommandDetailTembusan();
            context.ConsultingLetterOfCommands.Remove(consultingletterofcommand);
            var iduntuk = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID.Equals(id)).Select(p => p.ConsultingLetterOfCommandDetailUntukID).FirstOrDefault();
            var idtembusan = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID.Equals(id)).Select(p => p.ConsultingLetterOfCommandDetailTembusanID).FirstOrDefault();
            var iddasar = context.ConsultingLetterOfCommandDetailDasars.Where(p => p.ConsultingSuratPerintahID.Equals(id)).Select(p => p.ConsultingLetterOfCommandDetailDasarID).FirstOrDefault();
            var Ldasar = context.ConsultingLetterOfCommandDetailDasars.Find(iddasar);
            var Ltembusan = context.ConsultingLetterOfCommandDetailTembusans.Find(idtembusan);
            var Luntuk = context.ConsultingLetterOfCommandDetailUntuks.Find(iduntuk);
            context.ConsultingLetterOfCommandDetailDasars.Remove(Ldasar);
            context.ConsultingLetterOfCommandDetailTembusans.Remove(Ltembusan);
            context.ConsultingLetterOfCommandDetailUntuks.Remove(Luntuk);
            context.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Letter Of Command", consultingletterofcommand, conletcom, username);
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Letter Of Command Dasar", Ldasar, conletdas, username);
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Letter Of Command Untuk", Luntuk, conletunt, username);
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Letter Of Command Tembusan", Ltembusan, conlettem, username);
            TempData["message"] = "Consulting Letter Of Command successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult AutocompleteCR(string term)
        {
            var items = context.ConsultingRequests.Where(p => p.Status == "Approve").Select(p => p.NoRequest).ToList();

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
            var items = context.Employees.Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingLetterOfCommand consultingLetterOfCommand = await context.ConsultingLetterOfCommands.FindAsync(id);
            string letterOfCommandDetailDasar = context.ConsultingLetterOfCommandDetailDasars.Find(id).Dasar;
            string letterOfCommandDetailTembusan = context.ConsultingLetterOfCommandDetailTembusans.Find(id).Tembusan;
            string letterOfCommandDetailUntuk = context.ConsultingLetterOfCommandDetailUntuks.Find(id).Untuk;
            ViewBag.Dasar = letterOfCommandDetailDasar;
            ViewBag.Untuk = letterOfCommandDetailUntuk;
            ViewBag.Tembusan = letterOfCommandDetailTembusan;
            ViewBag.MemberName = consultingLetterOfCommand.MemberID;
            ViewBag.SupervisorName = consultingLetterOfCommand.SupervisorID;
            ViewBag.TeamLeaderName = consultingLetterOfCommand.TeamLeaderID;
            ViewBag.PICIDName = consultingLetterOfCommand.PicID;


            List<ConsultingLetterOfCommandDetailUntuk> untukList = new List<ConsultingLetterOfCommandDetailUntuk>();
            List<ConsultingLetterOfCommandDetailUntuk> Untuks = new List<ConsultingLetterOfCommandDetailUntuk>();
            List<ConsultingLetterOfCommandDetailTembusan> tembusanList = new List<ConsultingLetterOfCommandDetailTembusan>();
            List<ConsultingLetterOfCommandDetailTembusan> Tembusans = new List<ConsultingLetterOfCommandDetailTembusan>();

            Untuks = context.ConsultingLetterOfCommandDetailUntuks.Where(p => p.ConsultingSuratPerintahID.Equals(consultingLetterOfCommand.ConsultingSuratPerintahID)).ToList();
            foreach (var u in Untuks)
            {
                untukList.Add(u);
            }

            Tembusans = context.ConsultingLetterOfCommandDetailTembusans.Where(p => p.ConsultingSuratPerintahID.Equals(consultingLetterOfCommand.ConsultingSuratPerintahID)).ToList();
            foreach (var u in Tembusans)
            {
                tembusanList.Add(u);
            }

            if (consultingLetterOfCommand == null)
            {
                return HttpNotFound();
            }

            ViewBag.Untuks = untukList;
            ViewBag.Tembusans = tembusanList;

            ViewBag.WordDocumentFilename = "SuratPerintah";
            return View(consultingLetterOfCommand);
        }

        [HttpPost]
        public string GetActiv(string noreq)
        {
            ConsultingRequest Creuqest = context.ConsultingRequests.Where(p => p.NoRequest.Equals(noreq)).FirstOrDefault();
            if (Creuqest != null)
            {
                string activity = Creuqest.ActivityStr;
                return activity;
            }
            else return "";
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
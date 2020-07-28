using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Controllers
{
    [Authorize]
    public class ConsultingDraftAgreementsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private EmailController emailTransact = new EmailController();

        // GET: ConsultingDraftAgreements
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var consultingDraft = db.ConsultingDraftAgreements;
            return View(consultingDraft.ToList());
        }

        // GET: ConsultingDraftAgreements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingDraftAgreement consultingDraftAgreement = db.ConsultingDraftAgreements.Find(id);
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingDraftAgreement.RequesterID)).Select(p => p.Name).FirstOrDefault();
            if (consultingDraftAgreement == null)
            {
                return HttpNotFound();
            }
            return View(consultingDraftAgreement);
        }

        [WordDocument]
        public ActionResult Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingDraftAgreement consultingDraftAgreement = db.ConsultingDraftAgreements.Find(id);
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingDraftAgreement.RequesterID)).Select(p => p.Name).FirstOrDefault();
            if (consultingDraftAgreement == null)
            {
                return HttpNotFound();
            }
            ViewBag.WordDocumentFilename = "KesepakatanBaru";
            return View(consultingDraftAgreement);
        }

        // GET: ConsultingDraftAgreements/Create
        public ActionResult Create()
        {
            //ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name");
            return View();
        }

        // POST: ConsultingDraftAgreements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConsultingDraftAgreementID,NoRequest,NoSurat,RequesterID,Date_Start,ActivityStr,Tujuan,RuangLingkup,Peran,Status")] ConsultingDraftAgreement consultingDraftAgreement)
        {
            if (ModelState.IsValid)
            {
                db.ConsultingDraftAgreements.Add(consultingDraftAgreement);
                db.SaveChanges();
                TempData["message"] = "Draft Agreement successfully created!";
                return RedirectToAction("Index");
            }

            //ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", consultingDraftAgreement.ActivityID);
            return View(consultingDraftAgreement);
        }

        // GET: ConsultingDraftAgreements/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingDraftAgreement consultingDraftAgreement = db.ConsultingDraftAgreements.Find(id);
            if (consultingDraftAgreement == null)
            {
                return HttpNotFound();
            }
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingDraftAgreement.RequesterID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.datestart = consultingDraftAgreement.Date_Start.ToString("dd/MM/yyyy HH:mm");
            //ViewBag.dateend = consultingDraftAgreement.Date_End.ToString("dd/MM/yyyy HH:mm");
            return View(consultingDraftAgreement);
        }

        // POST: ConsultingDraftAgreements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConsultingDraftAgreementID,NoRequest,NoSurat,RequesterID,Date_Start,ActivityStr,Tujuan,RuangLingkup,Peran,Status")] ConsultingDraftAgreement consultingDraftAgreement, string submit)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    consultingDraftAgreement.Status = "Draft";
                else if (submit == "Send Back")
                    consultingDraftAgreement.Status = HelperController.GetStatusSendback(db, "Consulting Draft Agreement", consultingDraftAgreement.Status);
                else if (submit == "Approve")
                    consultingDraftAgreement.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    consultingDraftAgreement.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    consultingDraftAgreement.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {
                            Employee empl = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                            string emailContent = "Dear {0}, <BR/><BR/>Consulting Draft Agreement : {1} need your approval. Please click on this <a href=\"{2}\" title=\"CDA\">link</a> to show the Consulting Draft Agreement.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                            string urlRequest = baseUrl + "/ConsultingDraftAgreements/Details/" + consultingDraftAgreement.ConsultingDraftAgreementID;
                            emailTransact.SentEmailApproval(empl.Email, empl.Name, consultingDraftAgreement.NoSurat, emailContent, urlRequest);
                        }
                    }
                }
                ConsultingDraftAgreement oldData = db.ConsultingDraftAgreements.AsNoTracking().Where(p => p.ConsultingDraftAgreementID.Equals(consultingDraftAgreement.ConsultingDraftAgreementID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", consultingDraftAgreement.ConsultingDraftAgreementID, "Consulting Draft Agreement", oldData, consultingDraftAgreement, username);
                db.Entry(consultingDraftAgreement).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Draft Agreement successfully updated!";
                return RedirectToAction("Index");
            }
            //ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", consultingDraftAgreement.ActivityID);
            return View(consultingDraftAgreement);
        }

        // GET: ConsultingDraftAgreements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingDraftAgreement consultingDraftAgreement = db.ConsultingDraftAgreements.Find(id);
            if (consultingDraftAgreement == null)
            {
                return HttpNotFound();
            }
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingDraftAgreement.RequesterID)).Select(p => p.Name).FirstOrDefault();
            return View(consultingDraftAgreement);
        }

        // POST: ConsultingDraftAgreements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingDraftAgreement consultingDraftAgreement = db.ConsultingDraftAgreements.Find(id);
            ConsultingDraftAgreement condrag = new ConsultingDraftAgreement();
            db.ConsultingDraftAgreements.Remove(consultingDraftAgreement);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Draft Agreement", consultingDraftAgreement, condrag, username);
            TempData["message"] = "Draft Agreement successfully deleted!";
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
    }
}

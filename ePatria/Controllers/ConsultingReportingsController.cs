using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;

namespace ePatria.Controllers
{
    [Authorize()]
    public class ConsultingReportingsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        // GET: ConsultingReportings
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var consultingReportings = db.ConsultingReportings.Include(c => c.ConsultingLetterOfCommand);
            return View(consultingReportings.ToList());
        }

        // GET: ConsultingReportings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingReporting consultingReporting = db.ConsultingReportings.Find(id);
            if (consultingReporting == null)
            {
                return HttpNotFound();
            }
            return View(consultingReporting);
        }

        // GET: ConsultingReportings/Create
        public ActionResult Create()
        {
            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP");
            return View();
        }

        // POST: ConsultingReportings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "ConsultingReportingID,ConsultingSuratPerintahID,ActivityID,NoLaporan,Kepada,Dari,Lampiran,Perihal,Hasil")] ConsultingReporting consultingReporting)
        {
            if (ModelState.IsValid)
            {
                db.ConsultingReportings.Add(consultingReporting);
                db.SaveChanges();
                TempData["message"] = "Consulting Report successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingReporting.ConsultingSuratPerintahID);
            return View(consultingReporting);
        }

        // GET: ConsultingReportings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingReporting consultingReporting = db.ConsultingReportings.Find(id);
            if (consultingReporting == null)
            {
                return HttpNotFound();
            }
            return View(consultingReporting);
        }

        // POST: ConsultingReportings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "ConsultingReportingID,ConsultingSuratPerintahID,ActivityID,NoLaporan,Kepada,Dari,Lampiran,Perihal,Hasil")] ConsultingReporting consultingReporting)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                ConsultingReporting oldData = db.ConsultingReportings.AsNoTracking().Where(p => p.ConsultingReportingID.Equals(consultingReporting.ConsultingReportingID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", consultingReporting.ConsultingReportingID, "Consulting Reporting", oldData, consultingReporting, username);
                db.Entry(consultingReporting).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Consulting Report successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingReporting.ConsultingSuratPerintahID);
            return View(consultingReporting);
        }

        // GET: ConsultingReportings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingReporting consultingReporting = db.ConsultingReportings.Find(id);
            if (consultingReporting == null)
            {
                return HttpNotFound();
            }
            return View(consultingReporting);
        }

        // POST: ConsultingReportings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ConsultingReporting consultingReporting = db.ConsultingReportings.Find(id);
            db.ConsultingReportings.Remove(consultingReporting);
            db.SaveChanges();
            TempData["message"] = "Consulting Report successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult KepadaDariAutocomplete(string term)
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

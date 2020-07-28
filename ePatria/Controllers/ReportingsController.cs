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

namespace ePatria.Controllers
{
    [Authorize()]
    public class ReportingsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        // GET: Reportings
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            return View(await db.Reportings.ToListAsync());
        }

        // GET: Reportings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporting reporting = await db.Reportings.FindAsync(id);
            if (reporting == null)
            {
                return HttpNotFound();
            }
            return View(reporting);
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporting reporting = await db.Reportings.FindAsync(id);
            if (reporting == null)
            {
                return HttpNotFound();
            }
            ViewBag.fieldLetterNo = reporting.LetterOfCommand.NomorSP;
            ViewBag.dateLetterLHA = reporting.LetterOfCommand.AuditPlanningMemorandum.LHADateStart;

            ViewBag.auditDestination = reporting.LetterOfCommand.AuditPlanningMemorandum.TujuanAudit;
            ViewBag.rlDestination = reporting.LetterOfCommand.AuditPlanningMemorandum.RuangLingkupAudit;
            string x = reporting.Bab02Title;
            if (!x.Equals(null))
            {
                IQueryable<RCMDetailRiskControlIssue> rdrc = db.RCMDetailRiskControlIssue.Where(p => p.Title.Equals(x));// (nc => nc.Title.ToLower() == reporting.Bab02Title.ToLower()); //.Select(p => p.Title).FirstOrDefault();
                ViewBag.issueName = rdrc.Select(p => p.Title).FirstOrDefault();
                ViewBag.issueDesc = rdrc.Select(p => p.Fact).FirstOrDefault();
                ViewBag.impact = rdrc.Select(p => p.Impact).FirstOrDefault();
                ViewBag.signifikasi = rdrc.Select(p => p.Signification).FirstOrDefault() == null ? "" : rdrc.Select(p => p.Signification).FirstOrDefault();
                ViewBag.recommend = rdrc.Select(p => p.Recommendation).FirstOrDefault();
                ViewBag.responManagement = rdrc.Select(p => p.ManagementResponse).FirstOrDefault();
                ViewBag.statusManagement = rdrc.Select(p => p.Status).FirstOrDefault();
                ViewBag.pJ = rdrc.Select(p => p.PICID).FirstOrDefault();
                ViewBag.dueDate = rdrc.Select(p => p.Due_Date).FirstOrDefault();
            }
            else
            {
                ViewBag.issueName = '-';
                ViewBag.issueDesc = '-';
                ViewBag.impact = '-';
                ViewBag.signifikasi = '-';
                ViewBag.recommend = '-';
                ViewBag.responManagement = '-';
                ViewBag.statusManagement = '-';
                ViewBag.pJ = '-';
                ViewBag.dueDate = '-';
            }


            ViewBag.bab01Description = reporting.Bab01Description;
            ViewBag.bab02Description = reporting.Bab02Description;

            ViewBag.WordDocumentFilename = "LHA";
            return View(reporting);
        }

        // GET: Reportings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Reportings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ReportingID,LetterOfCommandID,NomorLaporan,MemoDescription,SummaryDescription,Bab01Title,Bab01SubTitle,Bab01Description,Bab02Title,Bab02SubTitle,Bab02Description")] Reporting reporting)
        {
            if (ModelState.IsValid)
            {
                db.Reportings.Add(reporting);
                await db.SaveChangesAsync();
                TempData["message"] = "Report successfully created!";
                return RedirectToAction("Index");
            }

            return View(reporting);
        }

        // GET: Reportings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporting reporting = await db.Reportings.FindAsync(id);
            reporting.ListBab = new List<ReportingBabModel>();
            //cari dan tambahakan bab
            int reportBab = 2;
            if (reporting.ReportingBabModel.Count() == 0)
            {
                List<RCMDetailRisk> rcmDR = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(reporting.FieldWork.RiskControlMatrixID)).ToList();
                if (rcmDR.Count() > 0)
                {
                    foreach (var dr in rcmDR)
                    {
                        List<RCMDetailRiskControl> rcmDRC = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(dr.RCMDetailRiskID)).ToList();
                        if (rcmDRC.Count() > 0)
                        {
                            foreach (var drc in rcmDRC)
                            {
                                List<RCMDetailRiskControlIssue> contIssue = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(drc.RCMDetailRiskControlID)).ToList();
                                if (contIssue.Count() > 0)
                                {
                                    foreach (var ci in contIssue)
                                    {
                                        reporting.ListBab.Add(new ReportingBabModel(ci) { Bab = reportBab });
                                        reportBab++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (reporting.ReportingBabModel.Count() > 0) {
                foreach (var item in reporting.ReportingBabModel)
                {
                    reporting.ListBab.Add(item);
                }
            }

            if (reporting == null)
            {
                return HttpNotFound();
            }
            return View(reporting);
        }

        // POST: Reportings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "ReportingID,LetterOfCommandID,NomorLaporan,MemoDescription,SummaryDescription,Bab01Title,Bab01SubTitle,Bab01Description,Bab02Title,Bab02SubTitle,Bab02Description,Fact,Criteria,Impact,impactValue,Cause,Recommendation,FieldWorkID")] Reporting reporting, string submit)
        public async Task<ActionResult> Edit( Reporting reporting, string submit)
        {
            if (ModelState.IsValid)
            {
                if (submit == "Generate Summary")
                    return RedirectToAction("Summary", new { id = reporting.ReportingID });
                else if (submit == "Generate Report")
                    return RedirectToAction("Report", new { id = reporting.ReportingID });
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                Reporting oldData = db.Reportings.AsNoTracking().Where(p => p.ReportingID.Equals(reporting.ReportingID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", reporting.ReportingID, "Reporting", oldData, reporting, username);
                reporting.ReportingBabModel.Clear();
                foreach (var item in reporting.ListBab)
                {
                    reporting.ReportingBabModel.Add(item);    
                }
                
                db.Entry(reporting).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["message"] = "Report successfully updated!";
                return RedirectToAction("Index");
            }
            return View(reporting);
        }

        // GET: Reportings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporting reporting = await db.Reportings.FindAsync(id);
            if (reporting == null)
            {
                return HttpNotFound();
            }
            return View(reporting);
        }

        // POST: Reportings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            Reporting reporting = await db.Reportings.FindAsync(id);
            Reporting report = new Reporting();
            db.Reportings.Remove(reporting);
            await db.SaveChangesAsync();
            auditTransact.CreateAuditTrail("Delete", id, "Reporting", reporting, report, username);
            TempData["message"] = "Report successfully deleted!";
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

        public ActionResult Bab02TitleAutocomplete(string term, int FieldWorkID)
        {
            List<string> items = new List<string>();
            FieldWork fieldwork = db.FieldWorks.Find(FieldWorkID);
            List<RCMDetailRisk> rcmDR = db.RCMDetailRisks.Where(p => p.RiskControlMatrixID.Equals(fieldwork.RiskControlMatrixID)).ToList();
            if (rcmDR.Count() > 0)
            {
                foreach (var dr in rcmDR)
                {
                    List<RCMDetailRiskControl> rcmDRC = db.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID.Equals(dr.RCMDetailRiskID)).ToList();
                    if (rcmDRC.Count() > 0)
                    {
                        foreach (var drc in rcmDRC)
                        {
                            List<RCMDetailRiskControlIssue> contIssue = db.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID.Equals(drc.RCMDetailRiskControlID)).ToList();
                            if (contIssue.Count() > 0)
                            {
                                foreach (var ci in contIssue)
                                    items.Add(ci.Title);
                            }
                        }
                    }
                }
            }

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public string GetDetailIssue(string issueTitle)
        {
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Where(p => p.Title.Equals(issueTitle)).FirstOrDefault();
            string result = String.Empty;
            if (issue != null)
                result = issue.Fact + ";" + issue.Criteria + ";" + issue.Impact + ";" + issue.ImpactValue + ";" + issue.Cause + ";" + issue.Recommendation;
            else
                result = ";;;;;";
            return result;
        }

        [WordDocument]
        public ActionResult Summary(int id)
        {
            Reporting reporting = db.Reportings.Find(id);
            ViewBag.WordDocumentFilename = "Generate_Summary_Report";
            return View(reporting);
        }

        [WordDocument]
        public ActionResult Report(int id)
        {
            Reporting reporting = db.Reportings.Find(id);
            ViewBag.WordDocumentFilename = "Generate_Report";
            return View(reporting);
        }
    }
}

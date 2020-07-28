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
    [Authorize]
    public class MonitoringTindakLanjutController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private FilesUploadController filesTransact = new FilesUploadController();
        
        public ActionResult Index()
        {
            var monitoring = db.MonitoringTindakLanjut;
            List<MonitoringTindakLanjut> monData = new List<MonitoringTindakLanjut>();
            if (TempData["Monitoring"] == null)
                monData = monitoring.ToList();
            else
            {
                monData = TempData["Monitoring"] as List<MonitoringTindakLanjut>;
            }
            //ViewBag.Monitoring = monData;
            return View(monData);
        }

        // GET: ExitMeetings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MonitoringTindakLanjut monitoring = db.MonitoringTindakLanjut.Find(id);
            if (monitoring == null)
            {
                return HttpNotFound();
            }
           
            return View(monitoring);
        }
        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCMDetailRiskControlIssue issue = db.RCMDetailRiskControlIssue.Find(id);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var iss = issue.NoRef.Split('/')[0] + issue.NoRef.Split('/')[1];
            var no = issue.NoRef.Split('/')[2];
            var getFiles = filesTransact.getFiles(iss + no, out newFilesName, out paths, url, server);

            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;
            if (issue.PICID != null)
                ViewBag.PICID = db.Employees.Where(p => p.Name.Equals(issue.PICID)).FirstOrDefault().Name;
            if (issue == null)
            {
                return HttpNotFound();
            }
            return View(issue);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RCMDetailRiskControlIssueID,RCMDetailRiskControlID,NoRef,Title,Fact,Criteria,Impact,ImpactValue,Cause,Recommendation,ManagementResponse,FindingType,Status,Status_Approval,Status_App,Signification,Due_Date,Close_Date,CaseClose")] RCMDetailRiskControlIssue issue, IEnumerable<HttpPostedFileBase> files, string PICID)
        {
            if (ModelState.IsValid)
            {
                MonitoringTindakLanjut dbitem = db.MonitoringTindakLanjut.Where(d => d.RCMDetailRiskControlIssueID == issue.RCMDetailRiskControlIssueID).FirstOrDefault();
                if (dbitem != null)
                {
                    if (issue.CaseClose == "true")
                        dbitem.Status = "Close";
                    else
                        dbitem.Status = "Open";
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Entry(dbitem).State = EntityState.Modified;
                    db.SaveChanges();    
                }
                

                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                RCMDetailRiskControlIssue oldData = db.RCMDetailRiskControlIssue.AsNoTracking().Where(p => p.RCMDetailRiskControlIssueID.Equals(issue.RCMDetailRiskControlIssueID)).FirstOrDefault();
                //auditTransact.CreateAuditTrail("Update", issue.RCMDetailRiskControlIssueID, "RCM Control Issue", oldData, issue, username);
                issue.PICID = PICID;
                db.Entry(issue).State = EntityState.Modified;
                db.SaveChanges();
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
                    var iss = issue.NoRef.Split('/')[0] + issue.NoRef.Split('/')[1];
                    var no = issue.NoRef.Split('/')[2];
                    bool getFiles = filesTransact.getFiles(iss + no, out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(iss + no, server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        var iss = issue.NoRef.Split('/')[0] + issue.NoRef.Split('/')[1];
                        var no = issue.NoRef.Split('/')[2];
                        bool addFile = filesTransact.addFile(iss + no, i, file, server);
                    }
                }
                Session.Remove("Images");
                return RedirectToAction("Index");
            }
            return View(issue);
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        public bool getMonitoring(string noref, string finding, string duedate, string engname, string nosp, string status, string closedate, string thnaudit)
        {
            List<MonitoringTindakLanjut> mon = db.MonitoringTindakLanjut.Where(p => p.RCMDetailRiskControlIssue.NoRef.Contains(noref)).ToList();
            if (finding != "")
                mon = mon.Where(p => p.RCMDetailRiskControlIssue.FindingType.Equals(finding)).ToList();
            if (duedate != "")
                mon = mon.Where(p => p.RCMDetailRiskControlIssue.Due_Date.Equals(duedate)).ToList();
            if (engname != "")
                mon = mon.Where(p => p.LetterOfCommand.Preliminary.EngagementActivity.Name.Equals(engname)).ToList();
            if (nosp != "")
                mon = mon.Where(p => p.LetterOfCommand.NomorSP.Equals(nosp)).ToList();
            if (status != "")
                mon = mon.Where(p => p.Status.Equals(status)).ToList();
            if (closedate != "")
                mon = mon.Where(p => p.RCMDetailRiskControlIssue.Close_Date.Equals(closedate)).ToList();
            TempData["Monitoring"] = mon;
            return true;
        }

        public ActionResult PICAutocomplete(string term)
        {
            var items = db.Employees.Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SPAutocomplete(string term)
        {
            var items = db.LetterOfCommands.Select(p => p.NomorSP).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EngAutocomplete(string term)
        {
            var items = db.EngagementActivities.Select(p => p.Name).ToList();

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
    }
}

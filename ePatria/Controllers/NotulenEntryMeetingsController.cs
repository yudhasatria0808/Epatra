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
using Novacode;
using System.Diagnostics;
using System.IO;

namespace ePatria.Controllers
{
    [Authorize()]
    public class NotulenEntryMeetingsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private FilesUploadController filesTransact = new FilesUploadController();

        // GET: NotulenEntryMeetings
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var notulenEntryMeetings = db.NotulenEntryMeetings.Include(n => n.LetterOfCommand);
            return View(await notulenEntryMeetings.ToListAsync());
        }

        // GET: NotulenEntryMeetings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NotulenEntryMeeting notulenEntryMeeting = await db.NotulenEntryMeetings.FindAsync(id);
            if (notulenEntryMeeting == null)
            {
                return HttpNotFound();
            }
            ViewBag.letterno = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(notulenEntryMeeting.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.Chairman = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeChairmanID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Moderator = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeModeratorID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Reporter = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeReporterID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Member = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeMemberID)).Select(p => p.Name).FirstOrDefault();

            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var notulen = "notulen";
            var no = notulenEntryMeeting.NotulenEntryMeetingID;
            bool getFiles = filesTransact.getFiles(notulen + no, out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;

            return View(notulenEntryMeeting);

        }

        // GET: NotulenEntryMeetings/Create
        public ActionResult Create()
        {
            ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM");
            return View();
        }

        // POST: NotulenEntryMeetings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string period1, string period2, string empchair, string empmoderator, string empreporter, string empmember, string letterno, [Bind(Include = "NotulenEntryMeetingID,AuditPlanningMemorandumID,Tujuan,TimeConsumableStartDate,TimeConsumableEndDate,Place,EmployeeChairmanID,EmployeeModeratorID,EmployeeReporterID,EmployeeMemberID,Opening,ExposurePlan,LetterOfCommandID")] NotulenEntryMeeting notulenEntryMeeting, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {   
                IQueryable<LetterOfCommand> loc = db.LetterOfCommands.Where(p => p.NomorSP.Equals(letterno));
                int LetterOfCommandID = 0;
                if (loc.Count() > 0 || letterno != null)
                    LetterOfCommandID = loc.Select(p => p.LetterOfCommandID).FirstOrDefault();
                notulenEntryMeeting.LetterOfCommandID = LetterOfCommandID;
                notulenEntryMeeting.TimeConsumableStartDate = Convert.ToDateTime(period1);
                notulenEntryMeeting.TimeConsumableEndDate = Convert.ToDateTime(period2);

                IQueryable<Employee> emp = db.Employees.Where(p => p.Name.Equals(empchair));
                int empId = 0;
                if (emp.Count() > 0 || emp != null)
                    empId = emp.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeChairmanID = empId;

                IQueryable<Employee> emp2 = db.Employees.Where(p => p.Name.Equals(empmoderator));
                int empId2 = 0;
                if (emp2.Count() > 0 || emp2 != null)
                    empId2 = emp2.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeModeratorID = empId2;

                IQueryable<Employee> emp3 = db.Employees.Where(p => p.Name.Equals(empreporter));
                int empId3 = 0;
                if (emp3.Count() > 0 || emp3 != null)
                    empId3 = emp3.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeReporterID = empId3;

                IQueryable<Employee> emp4 = db.Employees.Where(p => p.Name.Equals(empmember));
                int empId4 = 0;
                if (emp4.Count() > 0 || emp4 != null)
                    empId4 = emp4.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeMemberID = empId4;

                string username = User.Identity.Name;
                db.NotulenEntryMeetings.Add(notulenEntryMeeting);
                db.SaveChangesAsync();
                HttpServerUtilityBase server = Server;
                int i = 0;
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        var notulen = "notulen";
                        var id = notulenEntryMeeting.NotulenEntryMeetingID;
                        bool addFile = filesTransact.addFile(notulen + id, i, file, server);
                    }
                }
                NotulenEntryMeeting no = new NotulenEntryMeeting();
                auditTransact.CreateAuditTrail("Create", notulenEntryMeeting.NotulenEntryMeetingID, "NotulenEntryMeeting", no, notulenEntryMeeting, username);
                TempData["message"] = "Notulen Entry Meeting successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM", notulenEntryMeeting.AuditPlanningMemorandumID);
            return View(notulenEntryMeeting);
        }

        // GET: NotulenEntryMeetings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NotulenEntryMeeting notulenEntryMeeting = await db.NotulenEntryMeetings.FindAsync(id);
            if (notulenEntryMeeting == null)
            {
                return HttpNotFound();
            }

            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var notulen = "notulen";
            var no = notulenEntryMeeting.NotulenEntryMeetingID;
            var getFiles = filesTransact.getFiles(notulen + no, out newFilesName, out paths, url, server);

            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;

            ViewBag.letterno = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(notulenEntryMeeting.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.empchair = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeChairmanID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.empmoderator = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeModeratorID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.empreporter = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeReporterID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.empmember = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeMemberID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.datestart = notulenEntryMeeting.TimeConsumableStartDate.ToString("dd/MM/yyyy HH:mm");
            ViewBag.dateend = notulenEntryMeeting.TimeConsumableEndDate.ToString("dd/MM/yyyy HH:mm");

            return View(notulenEntryMeeting);
        }

        // POST: NotulenEntryMeetings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string period1, string period2, string empchair, string empmoderator, string empreporter, string empmember, string letterno, [Bind(Include = "NotulenEntryMeetingID,AuditPlanningMemorandumID,Tujuan,TimeConsumableStartDate,TimeConsumableEndDate,Place,EmployeeChairmanID,EmployeeModeratorID,EmployeeReporterID,EmployeeMemberID,Opening,ExposurePlan,LetterOfCommandID")] NotulenEntryMeeting notulenEntryMeeting, IEnumerable<HttpPostedFileBase> files)
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
                    var notulen = "notulen";
                    var id = notulenEntryMeeting.NotulenEntryMeetingID;
                    bool getFiles = filesTransact.getFiles(notulen + id, out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(notulen + id, server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        var notulen = "notulen";
                        var id = notulenEntryMeeting.NotulenEntryMeetingID;
                        bool addFile = filesTransact.addFile(notulen + id, i, file, server);
                    }
                }
                Session.Remove("Images");

                IQueryable<LetterOfCommand> loc = db.LetterOfCommands.Where(p => p.NomorSP.Equals(letterno));
                int LetterOfCommandID = 0;
                if (loc.Count() > 0 || letterno != null)
                    LetterOfCommandID = loc.Select(p => p.LetterOfCommandID).FirstOrDefault();
                notulenEntryMeeting.LetterOfCommandID = LetterOfCommandID;
                notulenEntryMeeting.TimeConsumableStartDate = Convert.ToDateTime(period1);
                notulenEntryMeeting.TimeConsumableEndDate = Convert.ToDateTime(period2);

                IQueryable<Employee> emp = db.Employees.Where(p => p.Name.Equals(empchair));
                int empId = 0;
                if (emp.Count() > 0 || emp != null)
                    empId = emp.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeChairmanID = empId;

                IQueryable<Employee> emp2 = db.Employees.Where(p => p.Name.Equals(empmoderator));
                int empId2 = 0;
                if (emp2.Count() > 0 || emp2 != null)
                    empId2 = emp2.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeModeratorID = empId2;

                IQueryable<Employee> emp3 = db.Employees.Where(p => p.Name.Equals(empreporter));
                int empId3 = 0;
                if (emp3.Count() > 0 || emp3 != null)
                    empId3 = emp3.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeReporterID = empId3;

                IQueryable<Employee> emp4 = db.Employees.Where(p => p.Name.Equals(empmember));
                int empId4 = 0;
                if (emp4.Count() > 0 || emp4 != null)
                    empId4 = emp4.Select(p => p.EmployeeID).FirstOrDefault();
                notulenEntryMeeting.EmployeeMemberID = empId4;

                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                NotulenEntryMeeting oldData = db.NotulenEntryMeetings.AsNoTracking().Where(p => p.NotulenEntryMeetingID.Equals(notulenEntryMeeting.NotulenEntryMeetingID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", notulenEntryMeeting.NotulenEntryMeetingID, "NotulenEntryMeeting", oldData, notulenEntryMeeting, username);
                db.Entry(notulenEntryMeeting).State = EntityState.Modified;
                db.SaveChangesAsync();
                TempData["message"] = "Notulen Entry Meeting successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM", notulenEntryMeeting.AuditPlanningMemorandumID);
            return View(notulenEntryMeeting);
        }

        // GET: NotulenEntryMeetings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NotulenEntryMeeting notulenEntryMeeting = await db.NotulenEntryMeetings.FindAsync(id);
            if (notulenEntryMeeting == null)
            {
                return HttpNotFound();
            }
            ViewBag.letterno = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(notulenEntryMeeting.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.Chairman = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeChairmanID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Moderator = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeModeratorID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Reporter = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeReporterID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Member = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeMemberID)).Select(p => p.Name).FirstOrDefault();
            return View(notulenEntryMeeting);
        }

        // POST: NotulenEntryMeetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            NotulenEntryMeeting notulenEntryMeeting = await db.NotulenEntryMeetings.FindAsync(id);
            NotulenEntryMeeting no = new NotulenEntryMeeting();
            db.NotulenEntryMeetings.Remove(notulenEntryMeeting);
            await db.SaveChangesAsync();
            auditTransact.CreateAuditTrail("Delete", id, "NotulenEntryMeeting", notulenEntryMeeting, no, username);
            TempData["message"] = "Notulen Entry Meeting successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult AutocompleteAPM(string term)
        {
            var items = db.LetterOfCommands.Where(p => p.Status.Equals("Approve")).Select(p => p.NomorSP).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AutocompleteEmp(string term)
        {
            var items = db.Employees.Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEngAPM(string apmid)
        {
            AuditPlanningMemorandumServices no = new AuditPlanningMemorandumServices();
            var noap = no.GetAuditPlanningMemorandum().Where(p => p.NomerAPM == apmid);

            IEnumerable<SelectListItem> apm = noap
            .Select(d => new SelectListItem
            {
                Value = d.Preliminary.EngagementID.ToString(),
                Text = d.Preliminary.EngagementActivity.Name

            });
            return Json(apm);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NotulenEntryMeeting notulenEntryMeeting = await db.NotulenEntryMeetings.FindAsync(id);
            if (notulenEntryMeeting == null)
            {
                return HttpNotFound();
            }

            ViewBag.letterno = db.LetterOfCommands.Where(p => p.LetterOfCommandID.Equals(notulenEntryMeeting.LetterOfCommandID)).Select(p => p.NomorSP).FirstOrDefault();
            ViewBag.Chairman = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeChairmanID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Moderator = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeModeratorID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Reporter = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeReporterID)).Select(p => p.Name).FirstOrDefault();
            ViewBag.Member = db.Employees.Where(p => p.EmployeeID.Equals(notulenEntryMeeting.EmployeeMemberID)).Select(p => p.Name).FirstOrDefault();

            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var notulen = "notulen";
            var no = notulenEntryMeeting.NotulenEntryMeetingID;
            bool getFiles = filesTransact.getFiles(notulen + no, out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;

            ViewBag.WordDocumentFilename = "NotulenEntryMeeting" + ViewBag.letterno;
            return View(notulenEntryMeeting);
        }

    }
}

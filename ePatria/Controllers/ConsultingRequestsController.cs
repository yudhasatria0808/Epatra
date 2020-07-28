using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using Microsoft.AspNet.Identity.Owin;
namespace ePatria.Controllers
{
    [Authorize]
    public class ConsultingRequestsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private FilesUploadController filesTransact = new FilesUploadController();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        // GET: ConsultingRequests
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var consultingRequests = db.ConsultingRequests.Include(c => c.Activity);
           
            return View(consultingRequests.ToList());
        }

        [WordDocument]
        public async Task<ActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingRequest consultingRequest = await db.ConsultingRequests.FindAsync(id);
            
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingRequest.RequesterID)).Select(p => p.Name).FirstOrDefault();
            if (consultingRequest == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.WordDocumentFilename = "ConsultingRequest";

            return View(consultingRequest);

        }

        // GET: ConsultingRequests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingRequest consultingRequest = db.ConsultingRequests.Find(id);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            bool getFiles = filesTransact.getFiles(consultingRequest.NoRequest.Replace("/",""), out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;


            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingRequest.RequesterID)).Select(p => p.Name).FirstOrDefault();
            if (consultingRequest == null)
            {
                return HttpNotFound();
            }
            return View(consultingRequest);
        }

        // GET: ConsultingRequests/Create
        public ActionResult Create()
        {
            ActivityServices activity = new ActivityServices();
            var activities = activity.GetActivity().Where(p => p.Status == "Approve");

            IEnumerable<SelectListItem> activityid = activities
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Name

            });
            ViewBag.ActivityID = activityid;
            String newReqNo = String.Empty;
            ConsultingRequest lastConReq = db.ConsultingRequests.OrderByDescending(p => p.ConsultingRequestID).FirstOrDefault();
            if (lastConReq != null)
            {
                if (lastConReq.NoRequest.Contains("/"))
                {
                    int reqNo = Convert.ToInt32(lastConReq.NoRequest.Split('/')[1]) + 1;
                    newReqNo = Convert.ToString("ConsultingRequest/" + reqNo);
                }
                else newReqNo = Convert.ToString("ConsultingRequest/" + 1);
            }
            else
                newReqNo = Convert.ToString("ConsultingRequest/" + 1);
            ViewBag.ReqNo = newReqNo;
            return View();
        }

        // POST: ConsultingRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string submit, string requester, [Bind(Include = "ConsultingRequestID,NoRequest,NoSurat,RequesterID,Date_Start,EvaluationResult,Status,ActivityStr")] ConsultingRequest consultingRequest, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                IQueryable<Employee> emp = db.Employees.Where(p => p.Name.Equals(requester));
                int empId = 0;
                if (emp.Count() > 0 || emp != null)
                    empId = emp.Select(p => p.EmployeeID).FirstOrDefault();

                consultingRequest.RequesterID = empId;

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    consultingRequest.Status = "Draft";
                else if (submit == "Send Back")
                    consultingRequest.Status = HelperController.GetStatusSendback(db, "Consulting Request", consultingRequest.Status);
                else if (submit == "Submit For Review By" + user)
                    consultingRequest.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    consultingRequest.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {
                            Employee empl = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                            string emailContent = "Dear {0}, <BR/><BR/>Consulting Request : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Consulting Request\">link</a> to show the Consulting Request.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                            string urlRequest = baseUrl + "/ConsultingRequests/Details/" + consultingRequest.ConsultingRequestID;
                            emailTransact.SentEmailApproval(empl.Email, empl.Name, consultingRequest.NoRequest, emailContent, urlRequest);
                        }
                    }
                }

                HttpServerUtilityBase server = Server;
                //var createFile = UploadFile(1234);
                int i = 0;
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(consultingRequest.NoRequest.Replace("/",""), i, file, server);
                    }
                }

                db.ConsultingRequests.Add(consultingRequest);
                db.SaveChanges();
                ConsultingRequest conreq = new ConsultingRequest();
                auditTransact.CreateAuditTrail("Create", consultingRequest.ConsultingRequestID, "Consulting Request", conreq, consultingRequest, username);
                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "draft";
                rrm.Description = page + consultingRequest.ConsultingRequestID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();
                TempData["message"] = "Request successfully created!";
                return RedirectToAction("Index");
            }

            //ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", consultingRequest.ActivityID);
            return View(consultingRequest);
        }

        // GET: ConsultingRequests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ConsultingRequest consultingRequest = db.ConsultingRequests.Find(id);

            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            var getFiles = filesTransact.getFiles(consultingRequest.NoRequest.Replace("/",""), out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;


            ActivityServices activity = new ActivityServices();
            var activities = activity.GetActivity().Where(p => p.Status == "Approve");

            IEnumerable<SelectListItem> activityid = activities
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Name

            });
            ViewBag.ActivityID = activityid;
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingRequest.RequesterID)).Select(p => p.Name).FirstOrDefault();
           
            string consultingRequestnosurat = db.ConsultingRequests.Find(id).NoSurat;
            string consultingRequestnorequest = db.ConsultingRequests.Find(id).NoRequest;
            //int consultingRequestrequest = db.ConsultingRequests.Find(id).RequesterID;
            //int consultingRequestactivity = db.ConsultingRequests.Find(id).ActivityID.Value;
            DateTime consultingRequeststart = db.ConsultingRequests.Find(id).Date_Start;
            //DateTime consultingRequestend = db.ConsultingRequests.Find(id).Date_End;
            ViewBag.NoSurat = consultingRequestnosurat;
            ViewBag.NoRequest = consultingRequestnorequest;
            //ViewBag.RequesterID = consultingRequestrequest;
            //ViewBag.ActivityID = consultingRequestactivity;
            ViewBag.Date_Start = consultingRequeststart;
            //ViewBag.Date_End = consultingRequestend;
            if (consultingRequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", consultingRequest.ActivityID);
            return View(consultingRequest);
        }

        // POST: ConsultingRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string submit, string requester, [Bind(Include = "ConsultingRequestID,NoRequest,NoSurat,RequesterID,Date_Start,EvaluationResult,Status,ActivityStr")] ConsultingRequest consultingRequest, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
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
                    bool getFiles = filesTransact.getFiles(consultingRequest.NoRequest.Replace("/", ""), out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(consultingRequest.NoRequest.Replace("/", ""), server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        bool addFile = filesTransact.addFile(consultingRequest.NoRequest.Replace("/", ""), i, file, server);
                    }
                }
                Session.Remove("Images");



                IQueryable<Employee> emp = db.Employees.Where(p => p.Name.Equals(requester));
                int empId = 0;
                if (emp.Count() > 0 || emp != null)
                    empId = emp.Select(p => p.EmployeeID).FirstOrDefault();
                consultingRequest.RequesterID = empId;

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    consultingRequest.Status = "Draft";
                else if (submit == "Send Back")
                    consultingRequest.Status = HelperController.GetStatusSendback(db, "Consulting Request", consultingRequest.Status);
                else if (submit == "Reject")
                    consultingRequest.Status = "Reject";
                else if (submit == "Approve")
                {
                    consultingRequest.Status = "Approve";
                    ConsultingDraftAgreement consultingDraftAgreement = new ConsultingDraftAgreement();
                    consultingDraftAgreement.NoRequest = consultingRequest.NoRequest;
                    consultingDraftAgreement.NoSurat = consultingRequest.NoSurat;
                    consultingDraftAgreement.RequesterID = consultingRequest.RequesterID;
                    consultingDraftAgreement.ActivityStr = consultingRequest.ActivityStr;
                    //consultingDraftAgreement.ActivityID = consultingRequest.ActivityID;
                    consultingDraftAgreement.Date_Start = consultingRequest.Date_Start;
                    //consultingDraftAgreement.Date_End = consultingRequest.Date_End;
                    consultingDraftAgreement.Status = "Draft";
                    db.ConsultingDraftAgreements.Add(consultingDraftAgreement);
                }
                else if (submit == "Submit For Review By" + user)
                    consultingRequest.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    consultingRequest.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {   
                                Employee empl = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                                string emailContent = "Dear {0}, <BR/><BR/>Consulting Request : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Consulting Request\">link</a> to show the Consulting Request.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string urlRequest = baseUrl + "/ConsultingRequests/Details/" + consultingRequest.ConsultingRequestID;
                                emailTransact.SentEmailApproval(empl.Email, empl.Name, consultingRequest.NoRequest, emailContent, urlRequest);
                        }
                    }
                }
                ConsultingRequest oldData = db.ConsultingRequests.AsNoTracking().Where(p => p.ConsultingRequestID.Equals(consultingRequest.ConsultingRequestID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", consultingRequest.ConsultingRequestID, "Consulting Request", oldData, consultingRequest, username);
                db.Entry(consultingRequest).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Request successfully updated!";
                return RedirectToAction("Index");
            }
            //ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", consultingRequest.ActivityID);
            return View(consultingRequest);
        }

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
        }

        // GET: ConsultingRequests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingRequest consultingRequest = db.ConsultingRequests.Find(id);
            if (consultingRequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.requester = db.Employees.Where(p => p.EmployeeID.Equals(consultingRequest.RequesterID)).Select(p => p.Name).FirstOrDefault();
            return View(consultingRequest);
        }

        // POST: ConsultingRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            ConsultingRequest consultingRequest = db.ConsultingRequests.Find(id);
            ConsultingRequest conreq = new ConsultingRequest();
            db.ConsultingRequests.Remove(consultingRequest);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Consulting Request", consultingRequest, conreq, username);
            TempData["message"] = "Request successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult Autocomplete(string term)
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

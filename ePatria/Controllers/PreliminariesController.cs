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
    public class PreliminariesController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private FilesUploadController filesTransact = new FilesUploadController();
        private AccountController account = new AccountController();
        PreliminaryServices mobjModel = new PreliminaryServices();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        // GET: Preliminaries
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var preliminaries = db.Preliminaries.Include(p => p.Activity);
            return View(await preliminaries.ToListAsync());
        }

        public Employee getEmployeeByUserName(string username)
        {
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(username).Result;
            string fullname = user.FirstName + " " + user.LastName;
            string email = user.Email;
            Employee emp = db.Employees.Where(p => p.Email.Equals(email) && p.Name.Equals(fullname)).FirstOrDefault();
            return emp;
        }

        // GET: Preliminaries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Preliminary preliminary = db.Preliminaries.Find(id);
            /*Employee emp = getEmployeeByUserName(User.Identity.Name);
            ViewBag.pic = false;
            ViewBag.supervisor = false;
            ViewBag.teamleader = false;
            ViewBag.member = false;
            if (preliminary.EngagementActivity.PICID != null)
            {
                if (preliminary.EngagementActivity.PICID.Equals(emp.Name))
                    ViewBag.pic = true;
            }
            if (preliminary.EngagementActivity.SupervisorID != null)
            {
                if (preliminary.EngagementActivity.SupervisorID.Equals(emp.Name))
                    ViewBag.supervisor = true;
            }
            if (preliminary.EngagementActivity.TeamLeaderID != null)
            {
                if (preliminary.EngagementActivity.TeamLeaderID.Equals(emp.Name))
                    ViewBag.teamleader = true;
            }
            if (preliminary.EngagementActivity.MemberID != null)
            {
                if (preliminary.EngagementActivity.MemberID.Contains(emp.Name))
                    ViewBag.member = true;
            }*/
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
            var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
            bool getFiles = filesTransact.getFiles(pre + no, out newFilesName, out paths, url, server);
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;
            if (preliminary == null)
            {
                return HttpNotFound();
            }
            return View(preliminary);
        }

        // GET: Preliminaries/Create
        public ActionResult Create()
        {   

            AnnualPlanningServices activity = new AnnualPlanningServices();
            var activities = activity.GetAnnualPlanning().Where(p => p.Approval_Status == "Approve");

            IEnumerable<SelectListItem> activityid = activities
             .Select(d => new SelectListItem
             {
                 Value = d.ActivityID.ToString(),
                 Text = d.Activity.Name

             });
            ViewBag.ActivityID = activityid;

            ActivityServices act = new ActivityServices();
            var acts = act.GetActivity().Where(p => p.Status == "Approve");

            IEnumerable<SelectListItem> actid = acts
            .Select(d => new SelectListItem
            {
                Value = d.ActivityID.ToString(),
                Text = d.Name

            });
            ViewBag.UnActivityID = actid;


            string prelimNo = String.Empty;

            Preliminary prelims = db.Preliminaries.OrderByDescending(p => p.PreliminaryID).FirstOrDefault();
            if (prelims != null)
            {
                int nopre = Convert.ToInt32(prelims.NomorPreliminarySurvey.Split('/')[1]) + 1;
                prelimNo = Convert.ToString("Preliminary/" + nopre);
            }
            else
                prelimNo = Convert.ToString("Preliminary/" + 1);

            ViewBag.preliminaryno = prelimNo;

            return View();
        }

        // POST: Preliminaries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string noprelim, int? ActivityID, string EngagementNameNull, string EngagementName, int? UnActivityID, int? engid, [Bind(Include = "PreliminaryID,Type,Status,NomorPreliminarySurvey,Date_Start,Date_End,EmployeeID,EngagementID,PICID,SupervisorID,TeamLeaderID,MemberID")] Preliminary preliminary,IEnumerable<HttpPostedFileBase> files, string submit, string[] member,
            string pic, string supervisor, string leader, string[] members)
        {
            if (ModelState.IsValid)
            {
                if (member != null)
                {
                    foreach (var mem in member)
                    {
                        preliminary.EmployeeID += mem + ";";
                    }                
                }

                preliminary.PICID = pic;
                preliminary.SupervisorID = supervisor;
                preliminary.TeamLeaderID = leader;
                if (members != null)
                {
                    foreach (var mem in members)
                    {
                        preliminary.MemberID += mem + ";";
                    }
                }

                HttpServerUtilityBase server = Server;
                int a = 0;
                if (engid == 0 && a < ActivityID)
                {
                    EngagementActivity engagement = new EngagementActivity();

                    engagement.ActivityID = ActivityID.Value;
                    engagement.Name = EngagementNameNull;
                    engagement.Date_Start = null;
                    engagement.Date_End = null;
                    db.EngagementActivities.Add(engagement);
                    db.SaveChanges();

                    preliminary.ActivityID = ActivityID.Value;
                    preliminary.EngagementID = engagement.EngagementID;
                    preliminary.NomorPreliminarySurvey = noprelim;
                     int i = 0;
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            i = i + 1;
                            var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                            var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                            bool addFile = filesTransact.addFile(pre + no, i, file, server);
                        }
                    }

                    string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                    if (submit == "Save")
                        preliminary.Status = "Draft";
                    else if(submit == "Send Back")
                        preliminary.Status = HelperController.GetStatusSendback(db, "Preliminary Survey",preliminary.Status);
                    else if (submit == "Approve")
                        preliminary.Status = "Approve";
                    else if (submit == "Submit For Review By" + user)
                        preliminary.Status = "Pending for Review by" + user;
                    else if (submit == "Submit For Approve By" + user)
                    {
                        preliminary.Status = "Pending for Approve by" + user;
                        string userToSentEmail = String.Empty;
                        if (user.Trim() == "CIA")
                        {
                            userToSentEmail = preliminary.PICID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Pengawas")
                        {
                            userToSentEmail = preliminary.SupervisorID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Ketua Tim")
                        {
                            userToSentEmail = preliminary.TeamLeaderID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Member")
                        {
                            userToSentEmail = preliminary.MemberID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                    }
                    db.Preliminaries.Add(preliminary);
                    db.SaveChanges();
                    string username = User.Identity.Name;
                    Preliminary pr = new Preliminary();
                    auditTransact.CreateAuditTrail("Create", preliminary.PreliminaryID, "Preliminary", pr, preliminary, username);

                    ReviewRelationMaster rrm = new ReviewRelationMaster();
                    string page = "prelim";
                    rrm.Description = page + preliminary.PreliminaryID;
                    db.ReviewRelationMasters.Add(rrm);
                    db.SaveChanges();

                    
                
                }else
                if (preliminary.Type == "UnPlanned")
                {
                    EngagementActivity engagement = new EngagementActivity();

                   
                    engagement.ActivityID = UnActivityID.Value;
                    engagement.Name = EngagementName;
                    engagement.Date_Start = null;
                    engagement.Date_End = null;
                    db.EngagementActivities.Add(engagement);
                    db.SaveChanges();

                    preliminary.ActivityID = UnActivityID.Value;
                    preliminary.EngagementID = engagement.EngagementID;
                    preliminary.NomorPreliminarySurvey = noprelim;
                    int i = 0;
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            i = i + 1;
                            var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                            var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                            bool addFile = filesTransact.addFile(pre + no, i, file, server);
                        }
                    }

                    string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                    if (submit == "Save")
                        preliminary.Status = "Draft";
                    else if (submit == "Send Back")
                        preliminary.Status = HelperController.GetStatusSendback(db, "Preliminary Survey",preliminary.Status);
                    else if (submit == "Approve")
                        preliminary.Status = "Approve";
                    else if (submit == "Submit For Review By" + user)
                        preliminary.Status = "Pending for Review by" + user;
                    else if (submit == "Submit For Approve By" + user)
                    {
                        preliminary.Status = "Pending for Approve by" + user;
                        string userToSentEmail = String.Empty;
                        if (user.Trim() == "CIA")
                        {
                            userToSentEmail = preliminary.PICID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Pengawas")
                        {
                            userToSentEmail = preliminary.SupervisorID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Ketua Tim")
                        {
                            userToSentEmail = preliminary.TeamLeaderID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Member")
                        {
                            userToSentEmail = preliminary.MemberID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                    }
                    db.Preliminaries.Add(preliminary);
                    db.SaveChanges();
                    string username = User.Identity.Name;
                    Preliminary pr = new Preliminary();
                    auditTransact.CreateAuditTrail("Create", preliminary.PreliminaryID, "Preliminary", pr, preliminary, username);
                    ReviewRelationMaster rrm = new ReviewRelationMaster();
                    string page = "prelim";
                    rrm.Description = page + preliminary.PreliminaryID;
                    db.ReviewRelationMasters.Add(rrm);

                    db.SaveChanges();
                   
                   

                }
                else //Type == "Planned"
                {
                    EngagementActivity engagement = new EngagementActivity();
                  
                    preliminary.ActivityID = ActivityID.Value;
                    preliminary.EngagementID = engid;
                    preliminary.NomorPreliminarySurvey = noprelim;
                    int i = 0;
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            i = i + 1;
                            var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                            var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                            bool addFile = filesTransact.addFile(pre + no, i, file, server);
                        }
                    }

                    string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                    if (submit == "Save")
                        preliminary.Status = "Draft";
                    else if (submit == "Send Back")
                        preliminary.Status = HelperController.GetStatusSendback(db, "Preliminary Survey", preliminary.Status);
                    else if (submit == "Approve")
                        preliminary.Status = "Approve";
                    else if (submit == "Submit For Review By" + user)
                        preliminary.Status = "Pending for Review by" + user;
                    else if (submit == "Submit For Approve By" + user)
                    {
                        preliminary.Status = "Pending for Approve by" + user;
                        string userToSentEmail = String.Empty;
                        if (user.Trim() == "CIA")
                        {
                            userToSentEmail = preliminary.PICID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Pengawas")
                        {
                            userToSentEmail = preliminary.SupervisorID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Ketua Tim")
                        {
                            userToSentEmail = preliminary.TeamLeaderID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                        else if (user.Trim() == "Member")
                        {
                            userToSentEmail = preliminary.MemberID;
                            if (userToSentEmail != null)
                                sentSingleEmailPrelim(userToSentEmail, preliminary);
                            else
                                sentEmailPrelim(preliminary, user.Trim());
                        }
                    }
                    db.Preliminaries.Add(preliminary);
                    db.SaveChanges();
                    string username = User.Identity.Name;
                    Preliminary pr = new Preliminary();
                    auditTransact.CreateAuditTrail("Create", preliminary.PreliminaryID, "Preliminary", pr, preliminary, username);
                    ReviewRelationMaster rrm = new ReviewRelationMaster();
                    string page = "prelim";
                    rrm.Description = page + preliminary.PreliminaryID;
                    db.ReviewRelationMasters.Add(rrm);
                    db.SaveChanges();                   
                }
                TempData["message"] = "Preliminary successfully created!";
                return RedirectToAction("Index");
            }



            return View(preliminary);
        }
   

        // GET: Preliminaries/Edit/5
        public ActionResult Edit(int? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Preliminary preliminary =  db.Preliminaries.Find(id);

            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;
            var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
            var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
            var getFiles = filesTransact.getFiles(pre + no, out newFilesName, out paths, url, server);
    
            ViewBag.newFilesName = newFilesName;
            ViewBag.paths = paths;

            if (preliminary.Type == "Planned")
            {
                AnnualPlanningServices activity = new AnnualPlanningServices();
                var activities = activity.GetAnnualPlanning().Where(p => p.ActivityID == preliminary.ActivityID);

                IEnumerable<SelectListItem> activityid = activities
                .Select(d => new SelectListItem
                {
                    Value = d.ActivityID.ToString(),
                    Text = d.Activity.Name

                });
                ViewBag.ActivityID = activityid;

                EngagementActivityServices Eng = new EngagementActivityServices();
                var Engs = Eng.GetEngagementActivity().Where(p => p.EngagementID == preliminary.EngagementID);

                IEnumerable<SelectListItem> EngID = Engs
                .Select(d => new SelectListItem
                {
                    Value = d.EngagementID.ToString(),
                    Text = d.Name

                });
                ViewBag.EngagementID = EngID;
            }
            else
            {
                int enggID = db.EngagementActivities.Find(preliminary.EngagementID).EngagementID;
                ViewBag.EnggaID = enggID;
                string EngName = db.EngagementActivities.Find(preliminary.EngagementID).Name;
                ViewBag.EngagementName = EngName;

                ActivityServices activity = new ActivityServices();
                var activities = activity.GetActivity().Where(p => p.ActivityID == preliminary.ActivityID);

                IEnumerable<SelectListItem> activityid = activities
                .Select(d => new SelectListItem
                {
                    Value = d.ActivityID.ToString(),
                    Text = d.Name

                });
                ViewBag.UnActivityID = activityid;

                //ViewBag.UnActivityID = new SelectList(db.Activities, "ActivityID", "Name");
            }

            return View(preliminary);
        }

        // POST: Preliminaries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? ActivityID, int? EnggaID, EngagementActivity engagementNam, string EngagementName, int? UnActivityID, string submit, [Bind(Include = "PreliminaryID,Type,Status,NomorPreliminarySurvey,Date_Start,Date_End,ReviewMasterID,ActivityID,EmployeeID,EngagementID,PICID,SupervisorID,TeamLeaderID")] Preliminary preliminary, IEnumerable<HttpPostedFileBase> files, string[] member, string[] members)
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
                    var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                    var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                    bool getFiles = filesTransact.getFiles(pre + no, out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(pre + no, server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                        var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                        bool addFile = filesTransact.addFile(pre + no, i, file, server);
                    }
                }
                Session.Remove("Images");

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    preliminary.Status = "Draft";
                else if (submit == "Send Back")
                    preliminary.Status = HelperController.GetStatusSendback(db, "Preliminary Survey", preliminary.Status);
                else if (submit == "Approve")
                    preliminary.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    preliminary.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    preliminary.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = preliminary.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    } else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = preliminary.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    } else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = preliminary.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    } else if (user.Trim() == "Member")
                    {
                        userToSentEmail = preliminary.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    }
                }

                if (preliminary.Type == "UnPlanned")
                {
                    preliminary.ActivityID = UnActivityID.Value;
                    preliminary.EngagementID = EnggaID;
                    engagementNam.Name = EngagementName;
                    engagementNam.ActivityID = UnActivityID.Value;
                    engagementNam.EngagementID = EnggaID.Value;
                    engagementNam.Date_Start = engagementNam.Date_Start;
                    engagementNam.Date_End = engagementNam.Date_End;

                    db.Entry(engagementNam).State = EntityState.Modified;
                }
                if (member != null)
                {
                    foreach (var mem in member)
                    {
                        preliminary.EmployeeID += mem + ";";
                    }                
                }

                if (members != null)
                {
                    foreach (var memb in members)
                    {
                        preliminary.MemberID += memb + ";";
                    }
                }

                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                Preliminary oldData = db.Preliminaries.AsNoTracking().Where(p => p.PreliminaryID.Equals(preliminary.PreliminaryID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", preliminary.PreliminaryID, "Preliminary", oldData, preliminary, username);
                db.Entry(preliminary).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Preliminary successfully updated!";
                return RedirectToAction("Index");
            }
            return View(preliminary);
        }

        private bool sentEmailPrelim(Preliminary preliminary, string user)
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
                        string emailContent = "Dear {0}, <BR/><BR/>Preliminary Survey : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Preliminary Survey\">link</a> to show the Preliminary Survey.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string urlPrelim = baseUrl + "/Preliminaries/Details/" + preliminary.PreliminaryID;
                        emailTransact.SentEmailApproval(emp.Email, emp.Name, preliminary.NomorPreliminarySurvey, emailContent, urlPrelim);
                    }
                }
            }
            return true;
        }

        private bool sentSingleEmailPrelim(string userToSentEmail, Preliminary preliminary)
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            Employee emp = db.Employees.Where(p => p.Name.Equals(userToSentEmail)).FirstOrDefault();
            if (emp != null)
            {
                string emailContent = "Dear {0}, <BR/><BR/>Preliminary Survey : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Preliminary Survey\">link</a> to show the Preliminary Survey.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string urlPrelim = baseUrl + "/Preliminaries/Details/" + preliminary.PreliminaryID;
                emailTransact.SentEmailApproval(emp.Email, emp.Name, preliminary.NomorPreliminarySurvey, emailContent, urlPrelim);
            }
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(string submit, [Bind(Include = "PreliminaryID,Type,Status,NomorPreliminarySurvey,Date_Start,Date_End,ReviewMasterID,ActivityID,EmployeeID,EngagementID,PICID,SupervisorID,TeamLeaderID,MemberID")] Preliminary preliminary, IEnumerable<HttpPostedFileBase> files)
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
                    var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                    var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                    bool getFiles = filesTransact.getFiles(pre + no, out newFilesName, out paths, url, server);
                    List<string> deletedFiles = paths.Except(keepImages).ToList();
                    bool deleteFiles = filesTransact.deleteFiles(deletedFiles, server);

                    i = filesTransact.getLastNumberOfFiles(pre + no, server);
                }
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        i = i + 1;
                        var pre = preliminary.NomorPreliminarySurvey.Split('/')[0];
                        var no = preliminary.NomorPreliminarySurvey.Split('/')[1];
                        bool addFile = filesTransact.addFile(pre + no, i, file, server);
                    }
                }
                Session.Remove("Images");

                string user = submit.Contains("By")  ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save")
                    preliminary.Status = "Draft";
                else if (submit == "Send Back")
                    preliminary.Status = HelperController.GetStatusSendback(db, "Preliminary Survey", preliminary.Status);
                else if (submit == "Approve")
                {
                    preliminary.Status = "Approve";
                    Walktrough walktrough = new Walktrough();
                    walktrough.PreliminaryID = preliminary.PreliminaryID;
                    walktrough.ActivityID = preliminary.ActivityID;
                    walktrough.Date_Start = preliminary.Date_Start;
                    walktrough.Date_End = preliminary.Date_End;
                    walktrough.Status = "Approve";
                    db.Walktroughs.Add(walktrough);
                }
                else if (submit == "Submit For Review By" + user)
                    preliminary.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    preliminary.Status = "Pending for Approve by" + user;
                    string userToSentEmail = String.Empty;
                    if (user.Trim() == "CIA")
                    {
                        userToSentEmail = preliminary.PICID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    }
                    else if (user.Trim() == "Pengawas")
                    {
                        userToSentEmail = preliminary.SupervisorID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    }
                    else if (user.Trim() == "Ketua Tim")
                    {
                        userToSentEmail = preliminary.TeamLeaderID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    }
                    else if (user.Trim() == "Member")
                    {
                        userToSentEmail = preliminary.MemberID;
                        if (userToSentEmail != null)
                            sentSingleEmailPrelim(userToSentEmail, preliminary);
                        else
                            sentEmailPrelim(preliminary, user.Trim());
                    }
                }
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                Preliminary oldData = db.Preliminaries.AsNoTracking().Where(p => p.PreliminaryID.Equals(preliminary.PreliminaryID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", preliminary.PreliminaryID, "Preliminary", oldData, preliminary, username);
                db.Entry(preliminary).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Preliminary successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ActivityID = new SelectList(db.Activities, "ActivityID", "Name", preliminary.ActivityID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "Type", preliminary.EmployeeID);
            return View(preliminary);
        }
      
        // GET: Preliminaries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Preliminary preliminary =  db.Preliminaries.Find(id);
            if (preliminary == null)
            {
                return HttpNotFound();
            }
            return View(preliminary);
        }

        // POST: Preliminaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            Preliminary preliminary = db.Preliminaries.Find(id);

            Preliminary emp = new Preliminary();
            db.Preliminaries.Remove(preliminary);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Preliminary", preliminary, emp, username);
            TempData["message"] = "Preliminary successfully deleted!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GetEng(int activid)
        {
            //List<EngagementActivity> obj = new List<EngagementActivity>();
            //obj = db.EngagementActivities.Where(m => m.ActivityID == activid).ToList();
            //SelectList obg = new SelectList(obj, "EngagementID", "Name", 0);

            var enggamenet = db.EngagementActivities.Where(p => p.ActivityID == activid).Select(p => new { engid = p.EngagementID, engname = p.Name }).ToList(); ;
            SelectList auditSelectList = new SelectList(enggamenet, "engid", "engname", 0);

            return Json(auditSelectList);
        }

        public string GetEngTeam(int engId)
        {
            string names = String.Empty;
            if (engId != 0)
            {
                EngagementActivity eng = db.EngagementActivities.Find(engId);
                var member = eng.MemberID;
                var teamleader = eng.TeamLeaderID;
                var supervisor = eng.SupervisorID;
                var pic = eng.PICID;
                if (teamleader == "" || supervisor == "" || pic == "" || member == "")
                {
                    //string memberName = "";
                    string teamleaderName = "";
                    string supervisorName = "";
                    string picName = "";
                    string memberName = "";
                    names = teamleaderName + "," + supervisorName + "," + picName + "," + memberName;
                }
                else
                {
                    names = teamleader + "," + supervisor + "," + pic + ',' + member;
                }
            }
            return names;
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

        public object[] getImages(object[] images)
        {
            Session["Images"] = images;
            return images;
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

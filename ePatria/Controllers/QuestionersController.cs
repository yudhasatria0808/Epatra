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
    public class QuestionersController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private AuditTrailsController auditTransact = new AuditTrailsController();


        // GET: Questioners
        public async Task<ActionResult> Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            return View(await db.Questioners.ToListAsync());
        }

        // GET: Questioners/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questioner questioner = await db.Questioners.FindAsync(id);
            if (questioner == null)
            {
                return HttpNotFound();
            }
            return View(questioner);
        }

        // GET: Questioners/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questioners/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string submit,[Bind(Include = "QuestionerID,Name,Type,Value,Status")] Questioner questioner,string Textarea,string Textbox,string Rating, string from, string to)
        {
            if (ModelState.IsValid)
            {
                if (questioner.Type == "TextBox")
                {
                    questioner.Value = Textbox;
                }
                else if (questioner.Type == "TextArea")
                {
                    questioner.Value = Textarea;
                }
                else
                {
                    questioner.Value = from + ";" + to;
                }
                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save" || submit == "Send Back")
                    questioner.Status = "Draft";
                else if (submit == "Approve")
                    questioner.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    questioner.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    questioner.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {
                            Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                            string emailContent = "Dear {0}, <BR/><BR/>Questioner : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Questioner\">link</a> to show the Questioner.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                            string url = baseUrl + "/Questioners/Details/" + questioner.QuestionerID;
                            //emailTransact.SentEmailApproval(emp.Email, emp.Name, questioner.Name, emailContent, url);
                        }
                    }
                }
                db.Questioners.Add(questioner);                
                await db.SaveChangesAsync();
                string username = User.Identity.Name;
                Questioner qus = new Questioner();
                auditTransact.CreateAuditTrail("Create", questioner.QuestionerID, "Questioner", qus, questioner, username);

                ReviewRelationMaster rrm = new ReviewRelationMaster();
                string page = "questioner";
                rrm.Description = page + questioner.QuestionerID;
                db.ReviewRelationMasters.Add(rrm);
                db.SaveChanges();

                TempData["message"] = "Questioner successfully created!";
                return RedirectToAction("Index");
            }

            return View(questioner);
        }

        // GET: Questioners/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questioner questioner = await db.Questioners.FindAsync(id);
            ViewBag.Textbox = questioner.Value;
            ViewBag.Textarea = questioner.Value;
            if (questioner.Value != null && questioner.Value.Contains(";"))
            {
                ViewBag.Rating1 = questioner.Value.Split(';')[0];
                ViewBag.Rating2 = questioner.Value.Split(';')[1];
            }
            ViewBag.Rating = questioner.Value;
            if (questioner == null)
            {
                return HttpNotFound();
            }
            return View(questioner);
        }

        // POST: Questioners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "QuestionerID,Name,Type,Value,Status")] Questioner questioner, string Textarea, string Textbox, string Rating, string submit, string Rating1, string Rating2)
        {
            if (ModelState.IsValid)
            {
                if (questioner.Type == "Textbox")
                {
                    questioner.Value = Textbox;
                }
                else if (questioner.Type == "Textarea")
                {
                    questioner.Value = Textarea;
                }
                else
                {
                    questioner.Value = Rating1 + ";" + Rating2;
                }

                string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
                if (submit == "Save" || submit == "Send Back")
                    questioner.Status = "Draft";
                else if (submit == "Approve")
                    questioner.Status = "Approve";
                else if (submit == "Submit For Review By" + user)
                    questioner.Status = "Pending for Review by" + user;
                else if (submit == "Submit For Approve By" + user)
                {
                    questioner.Status = "Pending for Approve by" + user;
                    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                    List<Employee> CIAEmpList = new List<Employee>();
                    if (CIAUserIds.Count() > 0)
                    {
                        var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                        foreach (var CIAUser in CIAUsers)
                        {
                            Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                            string emailContent = "Dear {0}, <BR/><BR/>Questioner : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Questioner\">link</a> to show the Questioner.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                            string url = baseUrl + "/Questioners/Details/" + questioner.QuestionerID;
                            //emailTransact.SentEmailApproval(emp.Email, emp.Name, questioner.Name, emailContent, url);
                        }
                    }
                }

                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                Questioner oldData = db.Questioners.AsNoTracking().Where(p => p.QuestionerID.Equals(questioner.QuestionerID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", questioner.QuestionerID, "Questioner", oldData, questioner, username);
                db.Entry(questioner).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["message"] = "Questioner successfully updated!";
                return RedirectToAction("Index");
            }
            return View(questioner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(string submit, string Textarea, string Textbox, string Rating, int QuestionerID)
        {
            var questioner = db.Questioners.Find(QuestionerID);
            string user = submit.Contains("By") ? submit.Split('y')[1] : String.Empty;
            if (submit == "Save" || submit == "Send Back")
                questioner.Status = "Draft";
            else if (submit == "Approve")
                questioner.Status = "Approve";
            else if (submit == "Submit For Review By" + user)
                questioner.Status = "Pending for Review by" + user;
            else if (submit == "Submit For Approve By" + user)
            {
                questioner.Status = "Pending for Approve by" + user;
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                List<string> CIAUserIds = Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles.Where(p => p.Name.Equals(user.Trim())).FirstOrDefault().Users.Select(p => p.UserId).ToList();
                List<Employee> CIAEmpList = new List<Employee>();
                if (CIAUserIds.Count() > 0)
                {
                    var CIAUsers = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(p => (CIAUserIds.Contains(p.Id))).ToList();
                    foreach (var CIAUser in CIAUsers)
                    {
                        Employee emp = db.Employees.Where(p => p.Email.Equals(CIAUser.Email)).FirstOrDefault();

                        string emailContent = "Dear {0}, <BR/><BR/>Questioner : {1} need your approval. Please click on this <a href=\"{2}\" title=\"Questioner\">link</a> to show the Questioner.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = baseUrl + "/Questioners/Details/" + questioner.QuestionerID;
                        //emailTransact.SentEmailApproval(emp.Email, emp.Name, questioner.Name, emailContent, url);
                    }
                }
            }
            string username = User.Identity.Name;
            db.Configuration.ProxyCreationEnabled = false;
            Questioner oldData = db.Questioners.AsNoTracking().Where(p => p.QuestionerID.Equals(questioner.QuestionerID)).FirstOrDefault();
            auditTransact.CreateAuditTrail("Update", questioner.QuestionerID, "Questioner", oldData, questioner, username);
            db.Entry(questioner).State = EntityState.Modified;
            await db.SaveChangesAsync();
            TempData["message"] = "Questioner successfully updated!";
            return RedirectToAction("Index");
            

        }

        // GET: Questioners/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questioner questioner = await db.Questioners.FindAsync(id);
            if (questioner == null)
            {
                return HttpNotFound();
            }
            return View(questioner);
        }

        // POST: Questioners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Questioner questioner = await db.Questioners.FindAsync(id);
            db.Questioners.Remove(questioner);
            string username = User.Identity.Name;
            Questioner qus = new Questioner();
            await db.SaveChangesAsync();
            auditTransact.CreateAuditTrail("Delete", id, "Questioner", questioner, qus, username);
            TempData["message"] = "Questioner successfully deleted!";
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

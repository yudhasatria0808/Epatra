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
    public class ConsultingFeedbacksController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private EmailController emailTransact = new EmailController();
        private static List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        private static List<char> characters = new List<char>() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '-', '_' };

        // GET: ConsultingFeedbacks
        public ActionResult Index()
        {

            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var consultingFeedbacks = db.ConsultingFeedbacks.Include(c => c.ConsultingLetterOfCommand);
            return View(consultingFeedbacks.ToList());
        }

        // GET: ConsultingFeedbacks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingFeedback consultingFeedback = db.ConsultingFeedbacks.Find(id);
            if (consultingFeedback == null)
            {
                return HttpNotFound();
            }
            return View(consultingFeedback);
        }

        // GET: ConsultingFeedbacks/Create
        public ActionResult Create()
        {
            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP");
            return View();
        }

        // POST: ConsultingFeedbacks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConsultingFeedbackID,ConsultingSuratPerintahID")] ConsultingFeedback consultingFeedback)
        {
            if (ModelState.IsValid)
            {
                db.ConsultingFeedbacks.Add(consultingFeedback);
                db.SaveChanges();
                TempData["message"] = "Consulting Feedback successfully created!";
                return RedirectToAction("Index");
            }

            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingFeedback.ConsultingSuratPerintahID);
            return View(consultingFeedback);
        }

        // GET: ConsultingFeedbacks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingFeedback consultingFeedback = db.ConsultingFeedbacks.Find(id);
            if (consultingFeedback == null)
            {
                return HttpNotFound();
            }
            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingFeedback.ConsultingSuratPerintahID);
            ViewBag.Questioners = new SelectList(db.Questioners.Where(p => p.Status.Equals("Approve")), "QuestionerID", "Name", 0);
           
            return View(consultingFeedback);
        }

        // POST: ConsultingFeedbacks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConsultingFeedbackID,ConsultingSuratPerintahID")] ConsultingFeedback consultingFeedback)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                ConsultingFeedback oldData = db.ConsultingFeedbacks.AsNoTracking().Where(p => p.ConsultingFeedbackID.Equals(consultingFeedback.ConsultingFeedbackID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", consultingFeedback.ConsultingFeedbackID, "Consulting Feedback", oldData, consultingFeedback, username);
                db.Entry(consultingFeedback).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Consulting Feedback successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ConsultingSuratPerintahID = new SelectList(db.ConsultingLetterOfCommands, "ConsultingSuratPerintahID", "NomorSP", consultingFeedback.ConsultingSuratPerintahID);
            return View(consultingFeedback);
        }

        // GET: ConsultingFeedbacks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsultingFeedback consultingFeedback = db.ConsultingFeedbacks.Find(id);
            if (consultingFeedback == null)
            {
                return HttpNotFound();
            }
            return View(consultingFeedback);
        }

        // POST: ConsultingFeedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ConsultingFeedback consultingFeedback = db.ConsultingFeedbacks.Find(id);
            db.ConsultingFeedbacks.Remove(consultingFeedback);
            db.SaveChanges();
            TempData["message"] = "Consulting Feedback successfully deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult PICAutocomplete(string term)
        {
            var items = db.Employees.Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public static string GetURL()
        {
            string URL = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < 11; i++)
            {
                int random = rand.Next(0, 3);
                if (random == 1)
                {
                    random = rand.Next(0, numbers.Count);
                    URL += numbers[random].ToString();
                }
                else
                {
                    random = rand.Next(0, characters.Count);
                    URL += characters[random].ToString();
                }
            }
            return URL;
        }

        public ActionResult SaveDetailFeedback(int ConsultingFeedbackID, string questions, string pics)
        {
            ConsultingFeedback feedback = db.ConsultingFeedbacks.Find(ConsultingFeedbackID);
            foreach (var pic in pics.Split(';'))
            {
                Employee emp = db.Employees.Where(p => p.Name.Equals(pic)).FirstOrDefault();
                ConsultingFeedbackQuestionDetail cfqd = new ConsultingFeedbackQuestionDetail();
                if (emp != null)
                {
                    cfqd.ConsultingFeedbackID = ConsultingFeedbackID;
                    cfqd.QuestionID = questions;
                    cfqd.PicID = emp.EmployeeID;
                    cfqd.Status = "Unsubmitted";
                    db.ConsultingFeedbackQuestionDetails.Add(cfqd);
                    db.SaveChanges();
                    ConsultingFeedbackQuestionDetail feed = db.ConsultingFeedbackQuestionDetails.Find(cfqd.ConsultingFeedbackQuestionDetailID);
                    feed.FeedbackURL = Request.Url.GetLeftPart(UriPartial.Authority) + "/ConsultingFeedbacks/FeedBackURL/" + feed.ConsultingFeedbackQuestionDetailID + "?empId=" + emp.EmployeeID;
                    feed.RandomURL = GetURL();
                    db.Entry(feed).State = EntityState.Modified;
                    db.SaveChanges();
                    if (!db.ListFeedbackSendedConsulting.Any(s => s.FeddbackId == feed.ConsultingFeedbackQuestionDetailID && s.PicId == feed.PicID))
                    {
                        string emailContent = "Dear {0}, <BR/><BR/>This is your consulting feedback URL, please click on this <a href=\"{1}\" title=\"your feedback\">link</a> to complete your answers.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.Url.GetLeftPart(UriPartial.Authority) + "/ConsultingFeedbacks/YourQuestionURL/" + feed.RandomURL;
                        emailTransact.SentEmailConsultingFeedback(emp.Email, emp.Name, emailContent, url);
                        db.ListFeedbackSendedConsulting.Add(new ListFeedbackSendedConsulting() { FeddbackId = feed.ConsultingFeedbackQuestionDetailID, PicId = feed.PicID });
                        db.SaveChanges();
                    }
                }
            }
            TempData["message"] = "Consulting Feedback successfully updated!";
            return RedirectToAction("Index");
        }


        public ActionResult ResendURL(int Id)
        {
            ConsultingFeedbackQuestionDetail feed = db.ConsultingFeedbackQuestionDetails.Find(Id);
            Employee emp = db.Employees.Find(feed.PicID);
            if (!db.ListFeedbackSendedConsulting.Any(s => s.FeddbackId == feed.ConsultingFeedbackQuestionDetailID && s.PicId == feed.PicID))
            {
                string emailContent = "Dear {0}, <BR/><BR/>This is your consulting feedback URL, please click on this <a href=\"{1}\" title=\"your feedback\">link</a> to complete your answers.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = Request.Url.GetLeftPart(UriPartial.Authority) + "/ConsultingFeedbacks/YourQuestionURL/" + feed.RandomURL;
                emailTransact.SentEmailConsultingFeedback(emp.Email, emp.Name, emailContent, url);
                TempData["message"] = "URL successfully resend!";
                db.ListFeedbackSendedConsulting.Add(new ListFeedbackSendedConsulting() { FeddbackId = feed.ConsultingFeedbackQuestionDetailID, PicId = feed.PicID });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public RedirectResult YourQuestionURL(string id)
        {
            ConsultingFeedbackQuestionDetail fqd = db.ConsultingFeedbackQuestionDetails.Where(p => p.RandomURL.Equals(id)).FirstOrDefault();
            return Redirect(fqd.FeedbackURL);
        }

        [AllowAnonymous]
        public ViewResult FeedBackURL(int id, int empId)
        {
            ConsultingFeedbackQuestionDetail fdq = db.ConsultingFeedbackQuestionDetails.Find(id);
            List<Questioner> questList = new List<Questioner>();
            ConsultingQuestionerAnswersViewModel q = new ConsultingQuestionerAnswersViewModel();
            Questioner quest = new Questioner();
            foreach (var question in fdq.QuestionID.Split(';'))
            {
                if (!String.IsNullOrEmpty(question))
                {
                    quest = db.Questioners.Find(Convert.ToInt32(question));
                    questList.Add(quest);
                }
            }
            ViewBag.EmpName = db.Employees.Find(fdq.PicID).Name;
            q.ConsultingQuestioners = questList;
            return View(q);
        }

        [HttpPost]
        public ActionResult FeedBackURL(ConsultingQuestionerAnswers qa)
        {
            ConsultingFeedbackQuestionDetail fqd = db.ConsultingFeedbackQuestionDetails.Find(qa.ConsultingFeedBackQuestionDetailID);
            fqd.Status = "Submitted";
            db.Entry(fqd).State = EntityState.Modified;
            db.SaveChangesAsync();
            TempData["message"] = "Consulting Feedback successfully updated!";
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

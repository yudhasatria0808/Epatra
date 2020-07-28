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
    public class FeedbacksController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        private static List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        private static List<char> characters = new List<char>() {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o','p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '-', '_'};
        // GET: Feedbacks
        public async Task<ActionResult> Index()
        {
            var feedbacks = db.Feedbacks.Include(f => f.FieldWork);
            return View(await feedbacks.ToListAsync());
        }

        // GET: Feedbacks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = await db.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        // GET: Feedbacks/Create
        public ActionResult Create()
        {
            ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM");
            return View();
        }

        // POST: Feedbacks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FeedbackID,FieldWorkID")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                db.Feedbacks.Add(feedback);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            //ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM", feedback.AuditPlanningMemorandumID);
            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = await db.Feedbacks.FindAsync(id);
            ViewBag.feedbackQ = feedback.FeedbackQuestionDetails.Select(p => p.QuestionerID).FirstOrDefault();
            if (feedback == null)
            {
                return HttpNotFound();
            }
            //ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM", feedback.AuditPlanningMemorandumID);
            ViewBag.Quests = db.Questioners.Where(p => p.Status.Equals("Approve")).ToList();
            ViewBag.Questss = new SelectList(db.Questioners.Where(p => p.Status.Equals("Approve")), "QuestionerID", "Name", 0);
            List<int> pics = db.FeedbackQuestionDetails.Where(p => p.FeedbackID == id).Select(p => p.PICID).ToList();
            List<string> emps = db.Employees.Where(p => (pics.Contains(p.EmployeeID))).Select(p => p.Name).ToList();
            ViewBag.pics = emps;
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FeedbackID,FieldWorkID")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                db.Entry(feedback).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //ViewBag.AuditPlanningMemorandumID = new SelectList(db.AuditPlanningMemorandums, "AuditPlanningMemorandumID", "NomerAPM", feedback.AuditPlanningMemorandumID);
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = await db.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Feedback feedback = await db.Feedbacks.FindAsync(id);
            db.Feedbacks.Remove(feedback);
            await db.SaveChangesAsync();
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

        public ActionResult PICAutocomplete(string term)
        {
            var items = db.Employees.Select(p => p.Name).ToList();

            var filteredItems = items.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public static string GetURL () {
            string URL = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < 11; i++) {
                int random = rand.Next(0, 3);
                if(random == 1) {
                    random = rand.Next(0, numbers.Count);
                    URL += numbers[random].ToString();
                } else {
                    random = rand.Next(0, characters.Count);
                    URL += characters[random].ToString();
                }
            }
            return URL;
        }

        public ActionResult SaveDetailFeedback(int feedbackID, string questions, string pics)
        {
            Feedback feedback = db.Feedbacks.Find(feedbackID);
            foreach (var pic in pics.Split(';'))
            {
                Employee emp = db.Employees.Where(p => p.Name.Equals(pic)).FirstOrDefault();
                FeedbackQuestionDetail fqd = new FeedbackQuestionDetail();
                if (emp != null)
                {
                    fqd.FeedbackID = feedbackID;
                    fqd.QuestionerID = questions;
                    fqd.PICID = emp.EmployeeID;
                    fqd.LetterofCommandID = feedback.FieldWork.LetterOfCommandID;
                    fqd.Status = "Unsubmitted";
                    db.FeedbackQuestionDetails.Add(fqd);
                    db.SaveChanges();
                    FeedbackQuestionDetail feed = db.FeedbackQuestionDetails.Find(fqd.FeedbackQuestionDetailID);
                    feed.FeedbackURL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Feedbacks/FeedBackURL/" + feed.FeedbackQuestionDetailID + "?empId=" + emp.EmployeeID;
                    feed.RandomURL = GetURL();
                    db.Entry(feed).State = EntityState.Modified;
                    db.SaveChanges();
                    if (!db.ListFeedbackSended.Any(s => s.FeddbackId == feed.FeedbackQuestionDetailID && s.PicId == feed.PICID)) {
                        string emailContent = "Dear {0}, <BR/><BR/>This is your feedback URL, please click on this <a href=\"{1}\" title=\"your feedback\">link</a> to complete your answers.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.Url.GetLeftPart(UriPartial.Authority) + "/Feedbacks/YourQuestionURL/" + feed.RandomURL;
                        emailTransact.SentEmailFeedback(emp.Email, emp.Name, emailContent, url);
                        db.ListFeedbackSended.Add(new ListFeedbackSended() { FeddbackId = feed.FeedbackQuestionDetailID, PicId = feed.PICID});
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult ResendURL(int Id)
        {
            FeedbackQuestionDetail feed = db.FeedbackQuestionDetails.Find(Id);
            Employee emp = db.Employees.Find(feed.PICID);
            if (!db.ListFeedbackSended.Any(s => s.FeddbackId == feed.FeedbackQuestionDetailID && s.PicId == feed.PICID))
            {
                string emailContent = "Dear {0}, <BR/><BR/>This is your feedback URL, please click on this <a href=\"{1}\" title=\"your feedback\">link</a> to complete your answers.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                string url = Request.Url.GetLeftPart(UriPartial.Authority) + "/Feedbacks/YourQuestionURL/" + feed.RandomURL;
                emailTransact.SentEmailFeedback(emp.Email, emp.Name, emailContent, url);
                db.ListFeedbackSended.Add(new ListFeedbackSended() { FeddbackId = feed.FeedbackQuestionDetailID, PicId = feed.PICID });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult DetailAnswers(int Id)
        {
            List<QuestionerAnswers> quest = db.QuestionerAnswers.Where(p => p.FeedBackQuestionDetailID == Id).ToList();
            return View(quest);
        }

        [AllowAnonymous]
        public RedirectResult YourQuestionURL(string id)
        {
            FeedbackQuestionDetail fqd = db.FeedbackQuestionDetails.Where(p => p.RandomURL.Equals(id)).FirstOrDefault();
            return Redirect(fqd.FeedbackURL);
        }

        [AllowAnonymous]
        public ViewResult FeedBackURL(int id, int empId)
        {   
            FeedbackQuestionDetail fdq = db.FeedbackQuestionDetails.Find(id);
            List<Questioner> questList = new List<Questioner>();
            QuestionerAnswersViewModel q = new QuestionerAnswersViewModel();
            Questioner quest = new Questioner();
            foreach (var question in fdq.QuestionerID.Split(';'))
            {
                if (!String.IsNullOrEmpty(question))
                {
                    quest = db.Questioners.Find(Convert.ToInt32(question));
                    questList.Add(quest);
                }
            }
            ViewBag.EmpName = db.Employees.Find(fdq.PICID).Name;
            q.Questioners = questList;
            return View(q);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FeedBackURL(QuestionerAnswers qa, string[] answer, int[] QuestionerID)
        {
            FeedbackQuestionDetail fqd = db.FeedbackQuestionDetails.Find(qa.FeedBackQuestionDetailID);
            fqd.Status = "Submitted";
            db.Entry(fqd).State = EntityState.Modified;
            int i = 0;
            foreach (var qId in QuestionerID)
            {
                QuestionerAnswers qAns = db.QuestionerAnswers.Where(p => p.FeedBackQuestionDetailID == qa.FeedBackQuestionDetailID && p.EmployeeID == qa.EmployeeID && p.QuestionerID == qId).FirstOrDefault();
                if (qAns == null)
                {
                    QuestionerAnswers qAnswer = new QuestionerAnswers();
                    qAnswer.Answer = answer[i];
                    qAnswer.EmployeeID = qa.EmployeeID;
                    qAnswer.FeedBackQuestionDetailID = qa.FeedBackQuestionDetailID;
                    qAnswer.QuestionerID = qId;
                    qAnswer.Status = "Submitted";
                    db.QuestionerAnswers.Add(qAnswer);
                }
                else {
                    qAns.Answer = answer[i];
                    qAns.Status = "Submitted";
                    db.Entry(qAns).State = EntityState.Modified;
                }
                i++;
            }
            db.SaveChanges();
            return View("Submitted");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ePatria.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ePatria.Controllers
{
    public class CommentController : ApiController
    {
        private string imgFolder = "/Images/profileimages/";
        //private string defaultAvatar = "user.png";
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();
        // GET api/Comment
        public IEnumerable<PostComment> GetPostComments()
        {
            var postcomments = db.PostComments.Include(p => p.Post);
            return postcomments.AsEnumerable();
        }

        // GET api/Comment/5
        public PostComment GetPostComment(int id)
        {
            PostComment postcomment = db.PostComments.Find(id);
            if (postcomment == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return postcomment;
        }

        // PUT api/Comment/5
        public HttpResponseMessage PutPostComment(int id, PostComment postcomment)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != postcomment.PostCommentId)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(postcomment).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        
        // POST api/Comment
        public HttpResponseMessage PostPostComment(PostComment postcomment)
        {
            //postcomment.CommentedBy = WebSecurity.CurrentUserId;
            postcomment.CommentedBy = User.Identity.GetUserName();
            //string strCurrentUserId = User.Identity.GetUserId();
            Post post = db.Posts.Find(postcomment.PostId);
            string Id = post.Message.Split(';')[1];
            postcomment.CommentedDate = DateTime.Now;
            ModelState.Remove("postcomment.CommentedBy");
            ModelState.Remove("postcomment.CommentedDate"); 
            if (ModelState.IsValid)
            {
                db.PostComments.Add(postcomment);
                db.SaveChanges();
                //var usr = db.UserProfiles.FirstOrDefault(x => x.UserProfileId == postcomment.CommentedBy);
                var ret = new
                {
                    CommentedBy = postcomment.CommentedBy,
                    CommentedByName = postcomment.CommentedBy,
                    CommentedByAvatar = imgFolder, //+(String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : postcomment.CommentedBy + "." + postcomment.UserProfile.AvatarExt),
                    CommentedDate = postcomment.CommentedDate,
                    CommentId = postcomment.PostCommentId,
                    Message = postcomment.Message,
                    PostId = postcomment.PostId
                };

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = postcomment.PostCommentId }));

                var commentBy = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId());
                string getInteger = new string(Id.Where(x => char.IsDigit(x)).ToArray());
                int id = Convert.ToInt32(getInteger);
                var postById = db.Employees.Where(p => p.Email.Equals(commentBy.Email)).Select(p => p.EmployeeID).FirstOrDefault();
                Employee empl = db.Employees.Find(Convert.ToInt32(postById));

                /*if (Id.Contains("apm"))
                {
                    AuditPlanningMemorandum apm = db.AuditPlanningMemorandums.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on APM : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show your reply for the APM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, apm.NomerAPM, content, postUrl, postcomment.Message);
                    if (apm.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(apm.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for APM : {1}. Someone had a reply comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                        emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                    }
                    if (apm.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(apm.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for APM : {1}. Someone had a reply comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                        emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                    }
                    if (apm.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(apm.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for APM : {1}. Someone had a reply comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                        emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                    }

                    if (apm.MemberID != null)
                    {
                        foreach (var mem in apm.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for APM : {1}. Someone had a reply comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                                emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("letter"))
                {
                    LetterOfCommand letter = db.LetterOfCommands.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on SP : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show your reply for the SP.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOFCommands/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, letter.NomorSP, content, postUrl, postcomment.Message);
                    if (letter.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for SP : {1}. Someone had a reply comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                    if (letter.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for SP : {1}. Someone had a reply comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                    if (letter.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for SP : {1}. Someone had a reply comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }

                    if (letter.MemberID != null)
                    {
                        foreach (var mem in letter.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for SP : {1}. Someone had a reply comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                                emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("prelim"))
                {
                    Preliminary pre = db.Preliminaries.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on Prelim : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show your reply for the APM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, pre.NomorPreliminarySurvey, content, postUrl, postcomment.Message);
                    if (pre.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(pre.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for Prelim : {1}. Someone had a reply comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                        emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                    }
                    if (pre.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(pre.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for Prelim : {1}. Someone had a reply comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                        emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                    }
                    if (pre.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(pre.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for Prelim : {1}. Someone had a reply comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                        emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                    }

                    if (pre.EngagementActivity.MemberID != null)
                    {
                        foreach (var mem in pre.EngagementActivity.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for Prelim : {1}. Someone had a reply comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                                emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("bpm"))
                {
                    BPM bpm = db.BPMs.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on BPM : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show your reply for the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, bpm.Name, content, postUrl, postcomment.Message);
                    if (bpm.Walktrough.Preliminary.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(bpm.Walktrough.Preliminary.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for BPM : {1}. Someone had a reply comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                        emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                    }
                    if (bpm.Walktrough.Preliminary.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(bpm.Walktrough.Preliminary.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for BPM : {1}. Someone had a reply comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                        emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                    }
                    if (bpm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(bpm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for BPM : {1}. Someone had a reply comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                        emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                    }

                    if (bpm.Walktrough.Preliminary.EngagementActivity.MemberID != null)
                    {
                        foreach (var mem in bpm.Walktrough.Preliminary.EngagementActivity.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for BPM : {1}. Someone had a reply comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                                emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("rcm"))
                {
                    RiskControlMatrix rcm = db.RiskControlMatrixs.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on RCM : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show your reply for the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                    string names = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, names, content, postUrl, postcomment.Message);
                    if (rcm.Walktrough.Preliminary.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(rcm.Walktrough.Preliminary.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for RCM : {1}. Someone had a reply comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                        string name = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                        emailTransact.SentEmailRCM(emp.Email, emp.Name, name, emailContent, url);
                    }
                    if (rcm.Walktrough.Preliminary.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(rcm.Walktrough.Preliminary.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for RCM : {1}. Someone had a reply comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                        string name = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                        emailTransact.SentEmailRCM(emp.Email, emp.Name, name, emailContent, url);
                    }
                    if (rcm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(rcm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for RCM : {1}. Someone had a reply comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                        string name = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                        emailTransact.SentEmailRCM(emp.Email, emp.Name, name, emailContent, url);
                    }

                    if (rcm.Walktrough.Preliminary.EngagementActivity.MemberID != null)
                    {
                        foreach (var mem in rcm.Walktrough.Preliminary.EngagementActivity.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for RCM : {1}. Someone had a reply comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                                string name = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                                emailTransact.SentEmailRCM(emp.Email, emp.Name, name, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("control"))
                {
                    RCMDetailRiskControl control = db.RCMDetailRiskControls.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on Control FieldWork : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show your reply for the Control.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    int IdFW = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdFW;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, control.ControlName, content, postUrl, postcomment.Message);
                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for Control FieldWork : {1}. Someone had a reply comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        int IdField = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdField;
                        emailTransact.SentEmailControl(emp.Email, emp.Name, control.ControlName, emailContent, url);
                    }
                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for Control FieldWork : {1}. Someone had a reply comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        int IdField = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdField;
                        emailTransact.SentEmailControl(emp.Email, emp.Name, control.ControlName, emailContent, url);
                    }
                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for Control FieldWork : {1}. Someone had a reply comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        int IdField = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdField;
                        emailTransact.SentEmailControl(emp.Email, emp.Name, control.ControlName, emailContent, url);
                    }

                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.MemberID != null)
                    {
                        foreach (var mem in control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for Control FieldWork : {1}. Someone had a reply comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                int IdField = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdField;
                                emailTransact.SentEmailControl(emp.Email, emp.Name, control.ControlName, emailContent, url);
                            }
                        }
                    }
                }
                if (Id.Contains("draft"))
                {
                    ConsultingDraftAgreement cda = db.ConsultingDraftAgreements.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a reply on Draft Agreement : {1}. <BR/><BR/> Your reply : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your Draft \">link</a> to show your reply for the Draft Agreement.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/ConsultingDraftAgreements/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, cda.NoRequest, content, postUrl, postcomment.Message);
                }*/
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Comment/5
        public HttpResponseMessage DeletePostComment(int id)
        {
            PostComment postcomment = db.PostComments.Find(id);
            if (postcomment == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.PostComments.Remove(postcomment);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, postcomment);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
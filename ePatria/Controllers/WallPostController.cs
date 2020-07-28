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
using System.Web.Routing;
using Microsoft.AspNet.Identity.Owin;


namespace ePatria.Controllers
{
    [Authorize]
    public class WallPostController : ApiController
    {
        //private string imgFolder = "/Images/profileimages/";
        private string imgFolder = "~/Content/assets/global/img/";
        private string defaultAvatar = "avatar3_small.jpg";
        private ePatriaDefault db = new ePatriaDefault();
        private EmailController emailTransact = new EmailController();

        // GET api/WallPost Untuk Annual Planning
        public dynamic GetPost(string id)
        {
            var ret = (from post in db.Posts.ToList() join revDet in db.ReviewRelationDetails on post.PostId equals revDet.PostId
                       join revMaster in db.ReviewRelationMasters on revDet.ReviewRelationMasterID equals revMaster.ReviewRelationMasterID where revMaster.Description == id 
                       orderby post.PostedDate descending
                       select new
                       {
                         Message = post.Message.Split(';')[0],
                         PostedBy = post.PostedBy,
                         PostedByName = post.PostedBy,
                         PostedByAvatar = imgFolder + defaultAvatar, //+ (String.IsNullOrEmpty(post.UserProfile.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
                         PostedDate = post.PostedDate,
                         PostId = post.PostId,
                         PostComments = from comment in post.PostComments.ToList()
                         orderby comment.CommentedDate
                                 select new
                                 {
                                  CommentedBy = comment.CommentedBy,
                                  CommentedByName = comment.CommentedBy,
                                  CommentedByAvatar = imgFolder, //+ (String.IsNullOrEmpty(comment.UserProfile.AvatarExt) ? defaultAvatar : comment.CommentedBy + "." + comment.UserProfile.AvatarExt),
                                  CommentedDate = comment.CommentedDate,
                                  CommentId = comment.PostCommentId,
                                  Message = comment.Message,
                                  PostId = comment.PostId

                                 }
                         }).AsEnumerable();
            return ret;
        }

        public HttpResponseMessage PostPost(Post post)
        {
            //reviewrel.ReviewRelationMasterID = ReviewRelationMasterID;
            post.PostedBy = User.Identity.GetUserName(); // GetUserId();
            post.PostedDate = DateTime.Now;

            ModelState.Remove("post.PostedBy");
            ModelState.Remove("post.PostedDate");
            string message = post.Message.Split(';')[0];
            string Id = post.Message.Split(';')[1];

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                int lastId = db.Posts.OrderByDescending(p => p.PostId).Select(p => p.PostId).FirstOrDefault();
                ReviewRelationDetail rrd = new ReviewRelationDetail();
                rrd.PostId = lastId;
                int rrmId = db.ReviewRelationMasters.Where(p => p.Description.Equals(Id)).Select(p => p.ReviewRelationMasterID).FirstOrDefault();
                rrd.ReviewRelationMasterID = rrmId;
                db.ReviewRelationDetails.Add(rrd);
                db.SaveChanges();

                //var usr = db.UserProfiles.FirstOrDefault(x => x.UserProfileId == post.PostedBy);
                var ret = new
                {
                    Message = message,
                    PostedBy = post.PostedBy,
                    PostedByName = User.Identity.GetUserName(),
                    PostedByAvatar = imgFolder, //+ (String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
                    PostedDate = post.PostedDate,
                    PostId = post.PostId
                };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = post.PostId }));

                var postBy = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId());
                string getInteger = new string(Id.Where(x => char.IsDigit(x)).ToArray());
                int id = Convert.ToInt32(getInteger);
                var postById = db.Employees.Where(p => p.Email.Equals(postBy.Email)).Select(p => p.EmployeeID).FirstOrDefault();
                Employee empl = db.Employees.Find(Convert.ToInt32(postById));
                /*if (Id.Contains("apm"))
                {
                    AuditPlanningMemorandum apm = db.AuditPlanningMemorandums.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on APM : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show your comment for the APM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, apm.NomerAPM, content, postUrl, message);

                    if (apm.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(apm.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for APM : {1}. Someone has comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                        emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                    }
                    if (apm.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(apm.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for APM : {1}. Someone has comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                        emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                    }
                    if (apm.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(apm.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for APM : {1}. Someone has comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for APM : {1}. Someone has comment on this APM. Please click on this <a href=\"{2}\" title=\"your APM\">link</a> to show the APM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/AuditPlanningMemorandums/Details/" + id;
                                emailTransact.SentEmailAPM(emp.Email, emp.Name, apm.NomerAPM, emailContent, url);
                            }
                        }                        
                    }
                }

                if (Id.Contains("letter"))
                {
                    LetterOfCommand letter = db.LetterOfCommands.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on SP : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show your comment for the SP.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, letter.NomorSP, content, postUrl, message);

                    if (letter.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                    if (letter.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                    if (letter.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                                emailTransact.SentEmailLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("prelim"))
                {
                    Preliminary pre = db.Preliminaries.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on Prelim : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show your comment for the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, pre.NomorPreliminarySurvey, content, postUrl, message);
                    if (pre.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(pre.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for Prelim : {1}. Someone has comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                        emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                    }
                    if (pre.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(pre.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for Prelim : {1}. Someone has comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                        emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                    }
                    if (pre.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(pre.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for Prelim : {1}. Someone has comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for Prelim : {1}. Someone has comment on this Prelim. Please click on this <a href=\"{2}\" title=\"your Prelim\">link</a> to show the Prelim.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Preliminaries/Details/" + id;
                                emailTransact.SentEmailPrelim(emp.Email, emp.Name, pre.NomorPreliminarySurvey, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("bpm"))
                {
                    BPM bpm = db.BPMs.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on BPM : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show your comment for the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, bpm.Name, content, postUrl, message);
                    if (bpm.Walktrough.Preliminary.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(bpm.Walktrough.Preliminary.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for BPM : {1}. Someone has comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                        emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                    }
                    if (bpm.Walktrough.Preliminary.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(bpm.Walktrough.Preliminary.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for BPM : {1}. Someone has comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                        emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                    }
                    if (bpm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(bpm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for BPM : {1}. Someone has comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for BPM : {1}. Someone has comment on this BPM. Please click on this <a href=\"{2}\" title=\"your BPM\">link</a> to show the BPM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/CreateBusiness/" + id + "?walkid=" + bpm.WalktroughID;
                                emailTransact.SentEmailBPM(emp.Email, emp.Name, bpm.Name, emailContent, url);
                            }
                        }
                    }
                }

                if (Id.Contains("rcm"))
                {
                    RiskControlMatrix rcm = db.RiskControlMatrixs.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on RCM : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show your comment for the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                    string names = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, names, content, postUrl, message);
                    if (rcm.Walktrough.Preliminary.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(rcm.Walktrough.Preliminary.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for RCM : {1}. Someone has comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                        string name = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                        emailTransact.SentEmailRCM(emp.Email, emp.Name, name, emailContent, url);
                    }
                    if (rcm.Walktrough.Preliminary.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(rcm.Walktrough.Preliminary.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for RCM : {1}. Someone has comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Walktroughs/Create/" + rcm.WalktroughID;
                        string name = db.BPMs.Where(p => p.BPMID.Equals(rcm.BusinessProces.BPMID)).Select(p => p.Name).FirstOrDefault();
                        emailTransact.SentEmailRCM(emp.Email, emp.Name, name, emailContent, url);
                    }
                    if (rcm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(rcm.Walktrough.Preliminary.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for RCM : {1}. Someone has comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for RCM : {1}. Someone has comment on this RCM. Please click on this <a href=\"{2}\" title=\"your RCM\">link</a> to show the RCM.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on Control FieldWork : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show your comment for the Control.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    int IdFW = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdFW;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, control.ControlName, content, postUrl, message);
                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.PICID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.PICID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for Control FieldWork : {1}. Someone has comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        int IdField = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdField;
                        emailTransact.SentEmailControl(emp.Email, emp.Name, control.ControlName, emailContent, url);
                    }
                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for Control FieldWork : {1}. Someone has comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        int IdField = db.FieldWorks.Where(p => p.RiskControlMatrixID.Equals(control.RCMDetailRisk.RiskControlMatrix.RiskControlMatrixID)).Select(p => p.FieldWorkID).FirstOrDefault();
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/FieldWorks/Edit/" + IdField;
                        emailTransact.SentEmailControl(emp.Email, emp.Name, control.ControlName, emailContent, url);
                    }
                    if (control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(control.RCMDetailRisk.RiskControlMatrix.Walktrough.Preliminary.EngagementActivity.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for Control FieldWork : {1}. Someone has comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for Control FieldWork : {1}. Someone has comment on this Control FieldWork. Please click on this <a href=\"{2}\" title=\"your Control\">link</a> to show the Control.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
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
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on Draft Agreement : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your Draft \">link</a> to show your reply for the Draft Agreement.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/ConsultingDraftAgreements/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, cda.NoRequest, content, postUrl, message);
                }

                if (Id.Contains("conleterdetail"))
                {
                    ConsultingLetterOfCommand letter = db.ConsultingLetterOfCommands.Find(id);
                    string content = "Dear {0}, <BR/><BR/>You had posted a comment on SP : {1}. <BR/><BR/> Your comment : <i>\"{3}\"</i>.<BR/><BR/>Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show your comment for the SP.<BR/><BR/><BR/> Regards,<BR/><BR/><BR/> ePatria Team";
                    string postUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                    emailTransact.SentEmailToCommentedUser(empl.Email, empl.Name, letter.NomorSP, content, postUrl, message);

                    if (letter.PicID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.PicID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Responsible person for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailConsultingLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                    if (letter.SupervisorID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.SupervisorID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Supervisor for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailConsultingLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }
                    if (letter.TeamLeaderID != null)
                    {
                        var employ = db.Employees.Where(p => p.Name.Equals(letter.TeamLeaderID)).Select(p => p.EmployeeID).FirstOrDefault();
                        Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                        string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Team Leader for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                        string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                        emailTransact.SentEmailConsultingLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                    }

                    if (letter.MemberID != null)
                    {
                        foreach (var mem in letter.MemberID.Split(';'))
                        {
                            if (!String.IsNullOrEmpty(mem))
                            {
                                var employ = db.Employees.Where(p => p.Name.Equals(mem)).Select(p => p.EmployeeID).FirstOrDefault();
                                Employee emp = db.Employees.Find(Convert.ToInt32(employ));
                                string emailContent = "Dear {0}, <BR/><BR/>You receive this email as you are registered as Member for SP : {1}. Someone has comment on this SP. Please click on this <a href=\"{2}\" title=\"your SP\">link</a> to show the SP.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team";
                                string url = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/LetterOfCommands/Details/" + id;
                                emailTransact.SentEmailConsultingLetter(emp.Email, emp.Name, letter.NomorSP, emailContent, url);
                            }
                        }
                    }
                }*/
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }                 
        }
        
        // POST api/WallPost
        //public HttpResponseMessage PostPost(Post post )
        //{
        //    post.PostedBy = User.Identity.GetUserName(); // GetUserId();
        //    post.PostedDate = DateTime.UtcNow;
        //    ModelState.Remove("post.PostedBy");
        //    ModelState.Remove("post.PostedDate");

        //    if (ModelState.IsValid)
        //    {
        //        db.Posts.Add(post);
        //        db.SaveChanges();
        //        var usr = db.UserProfiles.FirstOrDefault(x => x.UserProfileId == post.PostedBy);
        //        var ret = new
        //        {
        //            Message = post.Message,
        //            PostedBy = post.PostedBy,
        //            PostedByName = User.Identity.GetUserName(),
        //            PostedByAvatar = imgFolder, //+ (String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
        //            PostedDate = post.PostedDate,
        //            PostId = post.PostId
        //        };
        //        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
        //        response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = post.PostId }));
        //        return response;
        //    }
        //    else
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}

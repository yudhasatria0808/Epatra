using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Helpers;

namespace ePatria.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private HelperController Helper = new HelperController();
        public ActionResult Index()
        {   
            string uid = User.Identity.GetUserId();
            string roleId = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(uid).Roles.Select(p => p.RoleId).FirstOrDefault();
            string roleName = HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>().FindById(roleId).Name;
            string loggedInName = HelperController.getNameByUserName(User.Identity.Name);
            ViewBag.AP = new List<AnnualPlanning>();
            ViewBag.AU = new List<Activity>();
            ViewBag.Questioner = new List<Questioner>();
            ViewBag.Prelim = new List<Preliminary>();
            ViewBag.BPM = new List<BPM>();
            ViewBag.RCM = new List<RiskControlMatrix>();
            ViewBag.APM = new List<AuditPlanningMemorandum>();
            ViewBag.SP = new List<LetterOfCommand>();
            ViewBag.Fieldwork = new List<FieldWork>();
            ViewBag.EM = new List<ExitMeeting>();
            ViewBag.CR = new List<ConsultingRequest>();
            ViewBag.CDA = new List<ConsultingDraftAgreement>();
            ViewBag.CSP = new List<ConsultingLetterOfCommand>();
            ViewBag.CFW = new List<ConsultingFieldWork>();
            ViewBag.ListSp = db.LetterOfCommands.Where(d => d.PICID == loggedInName || d.SupervisorID == loggedInName || d.TeamLeaderID == loggedInName).ToList();
            //yudha
            
            
            int dataCount = 0;

            ViewBag.ConsulFWIssue = new List<ConsultingRCMDetailRiskControlIssue>();
            List<PermissionRoles> permConsulFWIssue = HelperController.getPermission(uid, "Consulting Control Issue");
            if (permConsulFWIssue != null)
            {
                List<string> userPerm = HelperController.getPermission(User.Identity.GetUserId(), "Consulting Control Issue").Select(d => d.roleID).ToList();
                List<string> ListroleName = new List<string>();
                bool isUpdate = permConsulFWIssue.Any(q => q.IsUpdate == true);
                List<ConsultingRCMDetailRiskControlIssue> data = new List<ConsultingRCMDetailRiskControlIssue>();
                foreach (string item in userPerm)
                {
                    string role_ = HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>().FindById(item).Name;
                    ListroleName.Add(role_);
                    data.AddRange(db.ConsultingRCMDetailRiskControlIssue.Where(p => p.Status_App.Contains(role_) || (isUpdate ? p.Status_App.Equals("Draft") : p.Status_App.Equals(""))).ToList());
                }

                //List<BPM> bpm = db.BPMs.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                //List<ConsultingRCMDetailRiskControlIssue> datalist = new List<ConsultingRCMDetailRiskControlIssue>();
                //if (ListroleName.Contains("Ketua Tim"))
                //    datalist = data.Where(p => (? loggedInName.Contains(p.Walktrough.Preliminary.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                //else if (ListroleName.Contains("Pengawas"))
                //    datalist = data.Where(p => (? loggedInName.Contains(p.Walktrough.Preliminary.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                //else if (ListroleName.Contains("CIA"))
                //    datalist = data.Where(p => (p.Walktrough.Preliminary.PICID != null ? loggedInName.Contains(p.Walktrough.Preliminary.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                //else datalist = data;
                ViewBag.ConsulFWIssue = data;
                dataCount += data.Count();
            }


            List<PermissionRoles> permAP = HelperController.getPermission(uid, "Annual Planning");
            if (permAP != null)
            {
                bool isUpdate = permAP.Any(q => q.IsUpdate == true);
                ViewBag.AP = db.AnnualPlannings.Where(p => p.Approval_Status.Contains(roleName) || (isUpdate ? p.Approval_Status.Equals("Draft") : p.Approval_Status.Equals(""))).ToList();
                dataCount += ViewBag.AP.Count;
            }

            List<PermissionRoles> permAU = HelperController.getPermission(uid, "Audit Universe");
            if (permAU != null)
            {
                bool isUpdate = permAU.Any(q => q.IsUpdate == true);
                ViewBag.AU = db.Activities.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                dataCount += ViewBag.AU.Count;
            }

            List<PermissionRoles> permQ = HelperController.getPermission(uid, "Questioner");
            if (permQ != null)
            {
                bool isUpdate = permQ.Any(q => q.IsUpdate == true);
                ViewBag.Questioner = db.Questioners.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                dataCount += ViewBag.Questioner.Count;
            }

            List<PermissionRoles> permPrelim = HelperController.getPermission(uid, "Preliminary Survey");
            if (permPrelim != null)
            {
                bool isUpdate = permPrelim.Any(q => q.IsUpdate == true);
                List<Preliminary> prelim = db.Preliminaries.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<Preliminary> prelimList = new List<Preliminary>();
                if (roleName == "Ketua Tim")
                    prelimList = prelim.Where(p => (p.TeamLeaderID != null ? loggedInName.Contains(p.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    prelimList = prelim.Where(p => (p.SupervisorID != null ? loggedInName.Contains(p.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    prelimList = prelim.Where(p => (p.PICID != null ? loggedInName.Contains(p.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else prelimList = prelim;
                ViewBag.Prelim = prelimList;
                dataCount += prelimList.Count();
            }

            List<PermissionRoles> permBPM = HelperController.getPermission(uid, "BPM");
            if (permBPM != null)
            {
                List<string> userPerm = HelperController.getPermission(User.Identity.GetUserId(), "BPM").Select(d => d.roleID).ToList();
                List<string> ListroleName = new List<string>();
                bool isUpdate = permBPM.Any(q => q.IsUpdate == true);
                List<BPM> bpm = new List<BPM>();
                foreach (string item in userPerm)
                {
                    string role_ = HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>().FindById(item).Name;
                    ListroleName.Add(role_);
                    bpm.AddRange(db.BPMs.Where(p => p.Status.Contains(role_) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList());
                }
                
                //List<BPM> bpm = db.BPMs.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<BPM> bpmList = new List<BPM>();
                if (ListroleName.Contains("Ketua Tim"))
                    bpmList = bpm.Where(p => (p.Walktrough.Preliminary.TeamLeaderID != null ? loggedInName.Contains(p.Walktrough.Preliminary.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (ListroleName.Contains("Pengawas"))
                    bpmList = bpm.Where(p => (p.Walktrough.Preliminary.SupervisorID != null ? loggedInName.Contains(p.Walktrough.Preliminary.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (ListroleName.Contains("CIA"))
                    bpmList = bpm.Where(p => (p.Walktrough.Preliminary.PICID != null ? loggedInName.Contains(p.Walktrough.Preliminary.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else bpmList = bpm;
                ViewBag.BPM = bpmList;
                dataCount += bpmList.Count();
            }
            
            List<PermissionRoles> permRCM = HelperController.getPermission(uid, "RCM");
            if (permRCM != null)
            {
                bool isUpdate = permRCM.Any(q => q.IsUpdate == true);
                List<RiskControlMatrix> rcm = db.RiskControlMatrixs.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<RiskControlMatrix> rcmList = new List<RiskControlMatrix>();
                if (roleName == "Ketua Tim")
                    rcmList = rcm.Where(p => (p.Walktrough.Preliminary.TeamLeaderID != null ? loggedInName.Contains(p.Walktrough.Preliminary.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    rcmList = rcm.Where(p => (p.Walktrough.Preliminary.SupervisorID != null ? loggedInName.Contains(p.Walktrough.Preliminary.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    rcmList = rcm.Where(p => (p.Walktrough.Preliminary.PICID != null ? loggedInName.Contains(p.Walktrough.Preliminary.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else rcmList = rcm;
                ViewBag.RCM = rcmList;
                dataCount += rcmList.Count();
            }

            List<PermissionRoles> permAPM = HelperController.getPermission(uid, "Audit Planning Memorandum");
            if (permAPM != null)
            {
                List<string> userPerm = HelperController.getPermission(User.Identity.GetUserId(), "BPM").Select(d => d.roleID).ToList();
                List<string> ListroleName = new List<string>();
                bool isUpdate = permAPM.Any(q => q.IsUpdate == true);
                List<AuditPlanningMemorandum> apm = new List<AuditPlanningMemorandum>();
                foreach (string item in userPerm)
                {
                    string role_ = HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>().FindById(item).Name;
                    ListroleName.Add(role_);
                    apm.AddRange(db.AuditPlanningMemorandums.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList());
                }
                //bool isUpdate = permAPM.Any(q => q.IsUpdate == true);
                //List<AuditPlanningMemorandum> apm = db.AuditPlanningMemorandums.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<AuditPlanningMemorandum> apmList = new List<AuditPlanningMemorandum>();
                if (ListroleName.Contains("Ketua Tim"))
                    apmList = apm.Where(p => (p.TeamLeaderID != null ? loggedInName.Contains(p.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (ListroleName.Contains("Pengawas"))
                    apmList = apm.Where(p => (p.SupervisorID != null ? loggedInName.Contains(p.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (ListroleName.Contains("CIA"))
                    apmList = apm.Where(p => (p.PICID != null ? loggedInName.Contains(p.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else apmList = apm;
                ViewBag.APM = apmList;
                dataCount += apmList.Count();
            }
            
            List<PermissionRoles> permSP = HelperController.getPermission(uid, "Surat Perintah");
            if (permSP != null)
            {
                bool isUpdate = permSP.Any(q => q.IsUpdate == true);
                List<LetterOfCommand> sp = db.LetterOfCommands.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<LetterOfCommand> spList = new List<LetterOfCommand>();
                if (roleName == "Ketua Tim")
                    spList = sp.Where(p => (p.TeamLeaderID != null ? loggedInName.Contains(p.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    spList = sp.Where(p => (p.SupervisorID != null ? loggedInName.Contains(p.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    spList = sp.Where(p => (p.PICID != null ? loggedInName.Contains(p.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else spList = sp;
                ViewBag.SP = spList;
                dataCount += spList.Count();
            }
            
            List<PermissionRoles> permFW = HelperController.getPermission(uid, "FieldWork");
            if (permFW != null)
            {
                bool isUpdate = permFW.Any(q => q.IsUpdate == true);
                List<FieldWork> fw = db.FieldWorks.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<FieldWork> fwList = new List<FieldWork>();
                if (roleName == "Ketua Tim")
                    fwList = fw.Where(p => (p.LetterOfCommand.TeamLeaderID != null ? loggedInName.Contains(p.LetterOfCommand.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    fwList = fw.Where(p => (p.LetterOfCommand.SupervisorID != null ? loggedInName.Contains(p.LetterOfCommand.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    fwList = fw.Where(p => (p.LetterOfCommand.PICID != null ? loggedInName.Contains(p.LetterOfCommand.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else fwList = fw;
                ViewBag.Fieldwork = fwList;
                dataCount += fwList.Count();
            }
            
            List<PermissionRoles> permEM = HelperController.getPermission(uid, "Exit Meeting");
            if (permEM != null)
            {
                bool isUpdate = permEM.Any(q => q.IsUpdate == true);
                List<ExitMeeting> em = db.ExitMeetings.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<ExitMeeting> emList = new List<ExitMeeting>();
                if (roleName == "Ketua Tim")
                    emList = em.Where(p => (p.LetterOfCommand.TeamLeaderID != null ? loggedInName.Contains(p.LetterOfCommand.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    emList = em.Where(p => (p.LetterOfCommand.SupervisorID != null ? loggedInName.Contains(p.LetterOfCommand.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    emList = em.Where(p => (p.LetterOfCommand.PICID != null ? loggedInName.Contains(p.LetterOfCommand.PICID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else emList = em;
                ViewBag.EM = emList;
                dataCount += emList.Count();
            }
            
            List<PermissionRoles> permCR = HelperController.getPermission(uid, "Consulting Request");
            if (permCR != null)
            {
                bool isUpdate = permCR.Any(q => q.IsUpdate == true);
                ViewBag.CR = db.ConsultingRequests.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                dataCount += ViewBag.CR.Count;
            }
            
            List<PermissionRoles> permCDA = HelperController.getPermission(uid, "Consulting Draft Agreement");
            if (permCDA != null)
            {
                bool isUpdate = permCDA.Any(q => q.IsUpdate == true);
                ViewBag.CDA = db.ConsultingDraftAgreements.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                dataCount += ViewBag.CDA.Count;
            }
            
            List<PermissionRoles> permCSP = HelperController.getPermission(uid, "Consulting Surat Perintah");
            if (permCSP != null)
            {
                bool isUpdate = permCSP.Any(q => q.IsUpdate == true);
                List<ConsultingLetterOfCommand> csp = db.ConsultingLetterOfCommands.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<ConsultingLetterOfCommand> cspList = new List<ConsultingLetterOfCommand>();
                if (roleName == "Ketua Tim")
                    cspList = csp.Where(p => (p.TeamLeaderID != null ? loggedInName.Contains(p.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    cspList = csp.Where(p => (p.SupervisorID != null ? loggedInName.Contains(p.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    cspList = csp.Where(p => (p.PicID != null ? loggedInName.Contains(p.PicID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else cspList = csp;
                ViewBag.CSP = cspList;
                dataCount += cspList.Count();
            }
            
            List<PermissionRoles> permCFW = HelperController.getPermission(uid, "Consulting Fieldwork");
            if (permCFW != null)
            {
                bool isUpdate = permCFW.Any(q => q.IsUpdate == true);
                List<ConsultingFieldWork> cfw = db.ConsultingFieldWork.Where(p => p.Status.Contains(roleName) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                List<ConsultingFieldWork> cfwList = new List<ConsultingFieldWork>();
                if (roleName == "Ketua Tim")
                    cfwList = cfw.Where(p => (p.ConsultingLetterOfCommand.TeamLeaderID != null ? loggedInName.Contains(p.ConsultingLetterOfCommand.TeamLeaderID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "Pengawas")
                    cfwList = cfw.Where(p => (p.ConsultingLetterOfCommand.SupervisorID != null ? loggedInName.Contains(p.ConsultingLetterOfCommand.SupervisorID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else if (roleName == "CIA")
                    cfwList = cfw.Where(p => (p.ConsultingLetterOfCommand.PicID != null ? loggedInName.Contains(p.ConsultingLetterOfCommand.PicID) : loggedInName.Contains("")) || (isUpdate ? p.Status.Equals("Draft") : p.Status.Equals(""))).ToList();
                else cfwList = cfw;
                ViewBag.CFW = cfwList;
                dataCount += cfwList.Count();
            }
            
            ViewBag.dataCount = dataCount;
            ViewBag.assDraft = getStatusTransaction("Draft", "Assurance");
            ViewBag.assCons = getStatusTransaction("Draft", "Consulting");
            ViewBag.actCount = getCount("Activity");
            return View();
        }

        public int getStatusTransaction(string status, string type)
        {
            int result = 0;
            if (type == "Assurance")
            {
                int pre = db.Preliminaries.Where(p => p.Status.Contains(status)).Count();
                int bpm = db.BPMs.Where(p => p.Status.Contains(status)).Count();
                int rcm = db.RiskControlMatrixs.Where(p => p.Status.Contains(status)).Count();
                int apm = db.AuditPlanningMemorandums.Where(p => p.Status.Contains(status)).Count();
                int sp = db.LetterOfCommands.Where(p => p.Status.Contains(status)).Count();
                int fw = db.FieldWorks.Where(p => p.Status.Contains(status)).Count();
                int aq = db.AuditQuery.Where(p => p.Status.Contains(status)).Count();
                int issue = db.RCMDetailRiskControlIssue.Where(p => p.Status_App.Contains(status)).Count();
                int em = db.ExitMeetings.Where(p => p.Status.Contains(status)).Count();
                result = pre + bpm + rcm + apm + sp + fw + aq + issue + em;
            }
            else if (type == "Consulting")
            {
                int req = db.ConsultingRequests.Where(p => p.Status.Contains(status)).Count();
                int cda = db.ConsultingDraftAgreements.Where(p => p.Status.Contains(status)).Count();
                int sp = db.ConsultingLetterOfCommands.Where(p => p.Status.Contains(status)).Count();
                int fw = db.ConsultingFieldWork.Where(p => p.Status.Contains(status)).Count();
                int aq = db.ConsultingAuditQuery.Where(p => p.Status.Contains(status)).Count();
                int issue = db.ConsultingRCMDetailRiskControlIssue.Where(p => p.Status_App.Contains(status)).Count();
                result = req + cda + sp + fw + aq + issue;
            }
            return result;
        }

        public int getCount(string model)
        { 
            int result = 0;
            if (model == "Activity")
                result = db.Activities.Count();
            return result;
        }

        /*[Authorize()]
        public ActionResult showChart1()
        {
            List<int> Count = new List<int>();
            int preCount = db.Preliminaries.Count();
            Count.Add(preCount);
            int waCount = db.Walktroughs.Count();
            Count.Add(waCount);
            int apmCount = db.AuditPlanningMemorandums.Count();
            Count.Add(apmCount);
            int spCount = db.LetterOfCommands.Count();
            Count.Add(spCount);
            int nemCount = db.NotulenEntryMeetings.Count();
            Count.Add(nemCount);
            int fwCount = db.FieldWorks.Count();
            Count.Add(fwCount);
            int emCount = db.ExitMeetings.Count();
            Count.Add(emCount);
            int rCount = db.Reportings.Count();
            Count.Add(rCount);
            var Chart = new Chart(width: 480, height: 300, theme: ChartTheme.Blue)
            .AddTitle("Jumlah Transaksi Assurance")
            .AddSeries(
                name: "Assurance",
                xValue: new[] {"Prelim","Walktrough","APM","SP","Notulen","Fieldwork","Exit Meeting", "Reporting"},
                yValues: Count )
            .GetBytes("png");
            return File(Chart, "image/png");
        }*/

        [Authorize]
        public JsonResult getChart1Data()
        {
            List<string> Count = new List<string>();
            int preCount = db.Preliminaries.Count();
            Count.Add("Prelim," + preCount);
            int waCount = db.Walktroughs.Count();
            Count.Add("Walktrough," + waCount);
            int apmCount = db.AuditPlanningMemorandums.Count();
            Count.Add("APM," + apmCount);
            int spCount = db.LetterOfCommands.Count();
            Count.Add("SP," + spCount);
            int nemCount = db.NotulenEntryMeetings.Count();
            Count.Add("Notulen," + nemCount);
            int fwCount = db.FieldWorks.Count();
            Count.Add("Fieldwork," + fwCount);
            int emCount = db.ExitMeetings.Count();
            Count.Add("ExitMeeting," + emCount);
            int rCount = db.Reportings.Count();
            Count.Add("Reporting," + rCount);
            return Json(Count, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult getChartAssuranceData()
        {
            List<object> Count = new List<object>();
            int preCount = db.Preliminaries.Count();
            Count.Add(new { transaction = "Prelim", total = preCount });
            int waCount = db.Walktroughs.Count();
            Count.Add(new { transaction = "Walktrough", total = waCount });
            int apmCount = db.AuditPlanningMemorandums.Count();
            Count.Add(new { transaction = "APM", total = apmCount });
            int spCount = db.LetterOfCommands.Count();
            Count.Add(new { transaction = "SP", total = spCount });
            int nemCount = db.NotulenEntryMeetings.Count();
            Count.Add(new { transaction = "Notulen", total = nemCount });
            int fwCount = db.FieldWorks.Count();
            Count.Add(new { transaction = "Fieldwork", total = fwCount });
            int emCount = db.ExitMeetings.Count();
            Count.Add(new { transaction = "EM", total = emCount });
            int rCount = db.Reportings.Count();
            Count.Add(new { transaction = "Reporting", total = rCount });
            return Json(Count , JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult getChartConsultingData()
        {
            List<object> Count = new List<object>();
            int crCount = db.ConsultingRequests.Count();
            Count.Add(new { transaction = "Request", total = crCount });
            int cdaCount = db.ConsultingDraftAgreements.Count();
            Count.Add(new { transaction = "Agreement", total = cdaCount });
            int cspCount = db.ConsultingLetterOfCommands.Count();
            Count.Add(new { transaction = "SP", total = cspCount });
            int cfwCount = db.ConsultingFieldWork.Count();
            Count.Add(new { transaction = "Fieldwork", total = cfwCount });
            int crepCount = db.ConsultingReportings.Count();
            Count.Add(new { transaction = "Reporting", total = crepCount });
            return Json(Count, JsonRequestBehavior.AllowGet);
        }

        /*[Authorize()]
        public ActionResult showChart2()
        {
            List<int> Count = new List<int>();
            int crCount = db.ConsultingRequests.Count();
            Count.Add(crCount);
            int cdaCount = db.ConsultingDraftAgreements.Count();
            Count.Add(cdaCount);
            int cspCount = db.ConsultingLetterOfCommands.Count();
            Count.Add(cspCount);
            int cfwCount = db.ConsultingFieldWork.Count();
            Count.Add(cfwCount);
            int crepCount = db.ConsultingReportings.Count();
            Count.Add(crepCount);
            int total = crCount + cdaCount + cspCount + cfwCount + crepCount;
            var Chart = new Chart(width: 480, height: 300, theme: ChartTheme.Green)
            .AddTitle("Jumlah Transaksi Consulting")
            .AddSeries(
            name: "Consulting", chartType: "Pie",
                xValue: new[] { "Request (" + (((Double)Count[0] / (Double)total) * 100).ToString("0.00") + "%)", "Draft Agreement (" + (((Double)Count[1] / (Double)total) * 100).ToString("0.00") + "%)"
                    , "SP (" + (((Double)Count[2] / (Double)total) * 100).ToString("0.00") + "%)", "Fieldwork ("+ (((Double)Count[3] / (Double)total) * 100).ToString("0.00") + "%)"
                    , "Reporting (" + (((Double)Count[4] / (Double)total) * 100).ToString("0.00") + "%)" },
                yValues: Count)
            .GetBytes("png");
            return File(Chart, "image/png");
        }*/

        [Authorize]
        public JsonResult getChartAllAudit()
        {
            List<object> Count = new List<object>();
            int currentCount = 0;
            int prev1Count = 0;
            int prev2Count = 0;
            int currentSPCount = db.LetterOfCommands.Where(p => p.Date_Start.Year.Equals(DateTime.Now.Year)).Count();
            int currentCSPCount = db.ConsultingLetterOfCommands.Where(p => p.StartDate.Year.Equals(DateTime.Now.Year)).Count();
            currentCount = currentSPCount + currentCSPCount;
            int prev1SPCount = db.LetterOfCommands.Where(p => p.Date_Start.Year.Equals(DateTime.Now.Year - 1)).Count();
            int prev1CSPCount = db.ConsultingLetterOfCommands.Where(p => p.StartDate.Year.Equals(DateTime.Now.Year - 1
                )).Count();
            prev1Count = prev1SPCount + prev1CSPCount;
            int prev2SPCount = db.LetterOfCommands.Where(p => p.Date_Start.Year.Equals(DateTime.Now.Year - 2)).Count();
            int prev2CSPCount = db.ConsultingLetterOfCommands.Where(p => p.StartDate.Year.Equals(DateTime.Now.Year - 2
                )).Count();
            prev2Count = prev2SPCount + prev2CSPCount;
            Count.Add(new { year = (DateTime.Now.Year - 2).ToString(), total = prev2Count });
            Count.Add(new { year = (DateTime.Now.Year - 1).ToString(), total = prev1Count });
            Count.Add(new { year = DateTime.Now.Year.ToString(), total = currentCount });
            return Json(Count, JsonRequestBehavior.AllowGet);
        }

        /*[Authorize]
        public ActionResult showChart3()
        {
            int currentCount = 0;
            int prev1Count = 0;
            int prev2Count = 0;
            int currentSPCount = db.LetterOfCommands.Where(p => p.Date_Start.Year.Equals(DateTime.Now.Year)).Count();
            int currentCSPCount = db.ConsultingLetterOfCommands.Where(p => p.StartDate.Year.Equals(DateTime.Now.Year)).Count();
            currentCount = currentSPCount + currentCSPCount;
            int prev1SPCount = db.LetterOfCommands.Where(p => p.Date_Start.Year.Equals(DateTime.Now.Year - 1)).Count();
            int prev1CSPCount = db.ConsultingLetterOfCommands.Where(p => p.StartDate.Year.Equals(DateTime.Now.Year - 1
                )).Count();
            prev1Count = prev1SPCount + prev1CSPCount;
            int prev2SPCount = db.LetterOfCommands.Where(p => p.Date_Start.Year.Equals(DateTime.Now.Year - 2)).Count();
            int prev2CSPCount = db.ConsultingLetterOfCommands.Where(p => p.StartDate.Year.Equals(DateTime.Now.Year - 2
                )).Count();
            prev2Count = prev2SPCount + prev2CSPCount;
            var Chart = new Chart(width: 480, height: 300, theme: ChartTheme.Yellow)
            .AddTitle("Jumlah Audit Per Tahun")
            .AddSeries(
            name: "Consulting", chartType: "Line",
                xValue: new[] { (DateTime.Now.Year - 2).ToString(), (DateTime.Now.Year - 1).ToString(), (DateTime.Now.Year).ToString() },
                yValues: new[] { prev2Count, prev1Count, currentCount })
            .GetBytes("png");
            return File(Chart, "image/png");
        }*/

        [Authorize()]
        public ActionResult Dashboard()
        {
            //dashboard code here
            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Sent()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailForm model)
        {
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress("yogiarjan@gmail.com"));  // replace with valid value 
                message.From = new MailAddress("sitpinus7@gmail.com");  // replace with valid value
                message.Subject = "Your email subject";
                message.Body = string.Format(body, model.FromName, model.FromEmail, model.Message);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "sitpinus7@gmail.com",  // replace with valid value
                        Password = "solusindoit"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    return RedirectToAction("Sent");
                }
            }
            return View(model);
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
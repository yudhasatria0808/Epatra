using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ePatria.Controllers
{
    [Authorize]
    public class DesignsController : Controller
    {
        private AuditTrailsController auditTransact = new AuditTrailsController();
        // GET: Designs
        public ActionResult Login_Index()
        {
            return View();
        }
        public ActionResult Dashboard_Index()
        {
            return View(auditTransact.GetAllAudit());
        }
        public ActionResult RoleManagement_Index()
        {
            return View();
        }
        public ActionResult RoleManagement_Create()
        {
            return View();
        }
        public ActionResult RoleManagement_Details()
        {
            return View();
        }
        public ActionResult RoleManagement_Edit()
        {
            return View();
        }
        public ActionResult ApprovalLevel_Index()
        {
            return View();
        }
        public ActionResult ApprovalLevel_Create()
        {
            return View();
        }
        public ActionResult ApprovalLevel_Details()
        {
            return View();
        }
        public ActionResult ApprovalLevel_Edit()
        {
            return View();
        }
        public ActionResult Employee_Index()
        {
            return View();
        }
        public ActionResult Employee_Create()
        {
            return View();
        }
        public ActionResult Employee_Details()
        {
            return View();
        }
        public ActionResult Employee_Edit()
        {
            return View();
        }
        public ActionResult UserManagement_Index()
        {
            return View();
        }
        public ActionResult UserManagement_Create()
        {
            return View();
        }
        public ActionResult UserManagement_Details()
        {
            return View();
        }
        public ActionResult UserManagement_Edit()
        {
            return View();
        }
        public ActionResult AnnualPlanning_Index()
        {
            return View();
        }
        public ActionResult AnnualPlanning_Create()
        {
            return View();
        }
        public ActionResult AnnualPlanning_Details()
        {
            return View();
        }
        public ActionResult AnnualPlanning_Edit()
        {
            return View();
        }
        public ActionResult Position_Index()
        {
            return View();
        }
        public ActionResult Organization_Index()
        {
            return View();
        }
        public ActionResult Organization_Create()
        {
            return View();
        }
        public ActionResult Organization_Details()
        {
            return View();
        }
        public ActionResult Organiation_Edit()
        {
            return View();
        }
        public ActionResult Position_Create()
        {
            return View();
        }
        public ActionResult Position_Details()
        {
            return View();
        }
        public ActionResult Position_Edit()
        {
            return View();
        }
        public ActionResult AuditUniverse_Index()
        {
            return View();
        }
        public ActionResult AuditUniverse_Create()
        {
            return View();
        }
        public ActionResult AuditUniverse_Details()
        {
            return View();
        }
        public ActionResult AuditUniverse_Edit()
        {
            return View();
        }
        public ActionResult Preparation_Index()
        {
            return View();
        }
        public ActionResult Preparation_Create()
        {
            return View();
        }
        public ActionResult Preparation_Details()
        {
            return View();
        }
        public ActionResult Preparation_Edit()
        {
            return View();
        }
        public ActionResult PreliminarySurvey_Index()
        {
            return View();
        }
        public ActionResult PreliminarySurvey_Details()
        {
            return View();
        }
        public ActionResult APM_Index()
        {
            return View();
        }
        public ActionResult APM_Details()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }
    }
}
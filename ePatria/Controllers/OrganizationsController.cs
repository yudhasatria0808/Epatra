using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria;
using ePatria.Models;
using Novacode;

namespace ePatria.Controllers
{
    [Authorize]
    public class OrganizationsController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        OrganizationServices mobjModel = new OrganizationServices();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        
        // GET: Organization Popup
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            string messageerror = TempData["messageerror"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            else if (!string.IsNullOrEmpty(messageerror))
            {
                ViewBag.messageerror = messageerror;
            }
            List<Organization> organizations = new List<Organization>();
            organizations = db.Organizations.ToList();
            ViewBag.OrganizationIndex = organizations;

            List<Organization> all = new List<Organization>();
            using (ePatriaDefault mydb = new ePatriaDefault())
            {
                all = mydb.Organizations.OrderBy(a => a.OrganizationParentID).ToList();
            }
            return View(all);
        }

        // Create Popup Organization
        public ActionResult CreateOrganization(int? organizationParentID)
        {
            if (ModelState.IsValid)
            {
                Organization orgObj = new Organization();
                ViewBag.OrganizationParentID = organizationParentID;
                ViewBag.IsUpdate = false;
                return View("CreateOrganization");
            }
            else

                return View();
        }


        // Create Sub Sub Organization
        public ActionResult CreateSubOrganization(int organizationid)
        {

            var data = mobjModel.GetOrganizationDetail(organizationid);
            Organization organization = db.Organizations.Find(organizationid);
            ViewBag.OrganizationParentID = organizationid;
            if (Request.IsAjaxRequest())
            {

                Organization orgObj = new Organization();

                orgObj.OrganizationParentID = data.OrganizationParentID;
                

                ViewBag.IsUpdate = true;
                return View("CreateSubOrganization");
            }
            else
                return View();       
        }
      

        [HttpPost]
        public ActionResult CreateOrganizations([Bind(Include = "OrganizationID,OrganizationParentID,Name,Description")] Organization organization, string Command)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Organizations.Add(organization);
                db.SaveChanges();
                Organization org = new Organization();
                auditTransact.CreateAuditTrail("Create", organization.OrganizationID, "Organization", org, organization, username);
                TempData["message"] = "Organization successfully created!";
                return RedirectToAction("Index");
            }
            return PartialView("CreateOrganization");
        }

        //public ActionResult EditOrganization(int organizationid)
        //{
        //    var data = mobjModel.GetOrganizationDetail(organizationid);

        //    if (Request.IsAjaxRequest())
        //    {
        //        Organization orgObj = new Organization();

        //        orgObj.OrganizationID = data.OrganizationID;
        //        orgObj.OrganizationParentID = data.OrganizationParentID;
        //        orgObj.Name = data.Name;
        //        orgObj.Description = data.Description;
               
        //        ViewBag.IsUpdate = true;
        //        return View("_EditOrganization", orgObj);
        //    }
        //    else
        //        return View(data);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrganizationID,OrganizationParentID,Name,Description")] Organization organization)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                Organization oldData = db.Organizations.AsNoTracking().Where(p => p.OrganizationID.Equals(organization.OrganizationID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", organization.OrganizationID, "Organization", oldData, organization, username);
                db.Entry(organization).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Organization successfully updated!";
                return RedirectToAction("Index");
            }
            return View(organization);
        }  

        //[HttpGet]
        //public ActionResult DeleteOrganization(int organizationid)
        //{
        //    bool check = mobjModel.DeleteOrganization(organizationid);
        //    var data = mobjModel.GetOrganization();
        //    ModelState.Clear();
        //    return RedirectToAction("Index", "Organizations");       
        //}

        [HttpGet]
        public ActionResult DeleteOrganization(int organizationid)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            Organization organization = db.Organizations.Find(organizationid);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            Organization org = new Organization();
            List<Organization> orgList = db.Organizations.Where(p => p.OrganizationParentID == organizationid).ToList();
            if (orgList.Count() > 0)
                TempData["messageerror"] = "Could not delete Organization due to having child!";
            else
            {
                db.Organizations.Remove(organization);
                db.SaveChanges();
                auditTransact.CreateAuditTrail("Delete", organizationid, "Organization", organization, org, username);
                TempData["message"] = "Organization successfully deleted!";
            }
            return RedirectToAction("Index");
        }


        // Controller Details 
        public ActionResult Details(int organizationid)
        {

            var data = mobjModel.GetOrganizationDetail(organizationid);
            Organization organization = db.Organizations.Find(organizationid);
            ViewBag.OrganizationIndex = organization;
            ViewBag.organization = organization;

            if (Request.IsAjaxRequest())
            {

                Organization orgObj = new Organization();

                orgObj.OrganizationID = data.OrganizationID;
                orgObj.OrganizationParentID = data.OrganizationParentID;
                orgObj.Name = data.Name;
                orgObj.Description = data.Description;

                ViewBag.IsUpdate = true;
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //return View("Details", orgObj);
            }
            else
                return View(data);
        }

        public ActionResult Edit(int organizationid)
        {

            var data = mobjModel.GetOrganizationDetail(organizationid);
            Organization organization = db.Organizations.Find(organizationid);
            ViewBag.OrganizationIndex = organization;
            ViewBag.organization = organization;

            if (Request.IsAjaxRequest())
            {

                Organization orgObj = new Organization();

                orgObj.OrganizationID = data.OrganizationID;
                orgObj.OrganizationParentID = data.OrganizationParentID;
                orgObj.Name = data.Name;
                orgObj.Description = data.Description;

                ViewBag.IsUpdate = true;
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //return View("Details", orgObj);
            }
            else
                return View(data);
        }


        
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using System.IO;

namespace ePatria.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private FilesUploadController filesTransact = new FilesUploadController();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        // GET: Employees
        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var employees = db.Employees.Include(e => e.Organizations).Include(e => e.Positions);
            return View(employees.ToList());
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            //List<string> newFilesName = new List<string>();
            //List<string> paths = new List<string>();
            //UrlHelper url = Url;
            //HttpServerUtilityBase server = Server;

            //bool getFiles = filesTransact.getFiles(employee.Name, out newFilesName, out paths, url, server);
            //ViewBag.newFilesName = newFilesName;
            //ViewBag.paths = paths;
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            ViewBag.OrganizationID = new SelectList(db.Organizations, "OrganizationID", "Name");
            ViewBag.PositionID = new SelectList(db.Positions, "PositionID", "PositionName");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployeeID,Type,Name,NoPEK,Email,PhoneNumber,Status,OrganizationID,PositionID")] Employee employee, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                bool nameExist = db.Employees.Any(p => p.Name == employee.Name);
                if (!nameExist)
                {
                    string username = User.Identity.Name;
                    db.Employees.Add(employee);
                    db.SaveChanges();
                    Employee emp = new Employee();
                    auditTransact.CreateAuditTrail("Create", employee.EmployeeID, "Employee", emp, employee, username);
                    TempData["message"] = "Employee successfully created!";
                    return RedirectToAction("Index");
                }
                else
                    ViewBag.NameExist = "Name already taken";
                
            }

            ViewBag.OrganizationID = new SelectList(db.Organizations, "OrganizationID", "Name", employee.OrganizationID);
            ViewBag.PositionID = new SelectList(db.Positions, "PositionID", "PositionName", employee.PositionID);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            //List<string> newFilesName = new List<string>();
            //List<string> paths = new List<string>();
            //UrlHelper url = Url;
            //HttpServerUtilityBase server = Server;

            //var getFiles = filesTransact.getFiles(employee.Name, out newFilesName, out paths, url, server);
            //ViewBag.newFilesName = newFilesName;
            //ViewBag.paths = paths;
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrganizationID = new SelectList(db.Organizations, "OrganizationID", "Name", employee.OrganizationID);
            ViewBag.PositionID = new SelectList(db.Positions, "PositionID", "PositionName", employee.PositionID);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployeeID,Type,Name,NoPEK,Email,PhoneNumber,Status,OrganizationID,PositionID,UserName")] Employee employee, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                db.Configuration.ProxyCreationEnabled = false;
                Employee oldData = db.Employees.AsNoTracking().Where(p => p.EmployeeID.Equals(employee.EmployeeID)).FirstOrDefault();
                string oldName = oldData.Name;
                bool nameExist = db.Employees.Any(p => p.Name == employee.Name);
                if (oldName != employee.Name)
                {
                    if (!nameExist)
                    {
                        string username = User.Identity.Name;
                        auditTransact.CreateAuditTrail("Update", employee.EmployeeID, "Employee", oldData, employee, username);
                        db.Entry(employee).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["message"] = "Employee successfully updated!";
                        return RedirectToAction("Index");
                    }
                    else
                        ViewBag.NameExist = "Name already taken";
                }
                else {
                    string username = User.Identity.Name;
                    auditTransact.CreateAuditTrail("Update", employee.EmployeeID, "Employee", oldData, employee, username);
                    db.Entry(employee).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["message"] = "Employee successfully updated!";
                    return RedirectToAction("Index");
                }
            }
            ViewBag.OrganizationID = new SelectList(db.Organizations, "OrganizationID", "Name", employee.OrganizationID);
            ViewBag.PositionID = new SelectList(db.Positions, "PositionID", "PositionName", employee.PositionID);
            return View(employee);
        }

        //public object[] getImages(object[] images)
        //{
        //    Session["Images"] = images;
        //    return images;
        //}

        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            Employee employee = db.Employees.Find(id);
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            UrlHelper url = Url;
            HttpServerUtilityBase server = Server;

            bool getFiles = filesTransact.getFiles(employee.Name, out newFilesName, out paths, url, server);
            bool deleteFiles = filesTransact.deleteFiles(paths, server);

            Employee emp = new Employee();
            db.Employees.Remove(employee);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Employee", employee, emp, username);
            TempData["message"] = "Employee successfully deleted!";
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

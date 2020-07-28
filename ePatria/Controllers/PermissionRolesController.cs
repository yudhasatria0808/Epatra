using ePatria.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace ePatria.Controllers
{
    [Authorize(Roles = "Admin, Superadmin")]
    public class PermissionRolesController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();

        public ActionResult Index()
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            var permissionRoles = db.PermissionRoles.ToList();
            return View(permissionRoles);
        }

        public ActionResult Create()
        {
            ViewBag.roleID = new SelectList(HttpContext.Request.GetOwinContext().GetUserManager<ApplicationRoleManager>().Roles, "Id", "Name", 0);
            ViewBag.permissionID = new SelectList(db.Permissions, "permissionID", "PermissionName");
            var perm = db.Permissions.ToList();
            ViewBag.Permissions = perm;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PermissionRoleID,PermissionID,IsCreate,IsRead,IsUpdate,IsDelete,IsSubmit1,IsSubmit2,IsApprove,roleID,Desc,Status")] PermissionRoles permRole)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.PermissionRoles.Add(permRole);
                db.SaveChanges();
                PermissionRoles pos = new PermissionRoles();
                auditTransact.CreateAuditTrail("Create", permRole.permissionID, "Role Permission", pos, permRole, username);
                TempData["message"] = "Role Permission successfully created!";
                return RedirectToAction("Index");
            }
            return View(permRole);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermissionRoles permission = db.PermissionRoles.Find(id);
            return View(permission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PermissionRoleID,permissionID,IsCreate,IsRead,IsUpdate,IsDelete,IsSubmit1,IsSubmit2,IsApprove,roleID")] PermissionRoles permissionRole)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                db.Configuration.ProxyCreationEnabled = false;
                PermissionRoles oldData = db.PermissionRoles.AsNoTracking().Where(p => p.PermissionRoleID.Equals(permissionRole.PermissionRoleID)).FirstOrDefault();
                auditTransact.CreateAuditTrail("Update", permissionRole.PermissionRoleID, "Role Permission", oldData, permissionRole, username);
                db.Entry(permissionRole).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Role Permission successfully updated!";
                return RedirectToAction("Index");
            }
            return View(permissionRole);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult SavePerm(string permName, string desc)
        {
            Permissions newPerm = new Permissions();
            newPerm.PermissionName = permName;
            newPerm.Desc = desc;
            newPerm.Status = "Active";
            db.Permissions.Add(newPerm);
            db.SaveChanges();
            var perm = db.Permissions.OrderBy(p => p.PermissionName).ToList();
            return Json(new { perm = perm }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditPermission(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permissions perm = db.Permissions.Find(id);
            return View(perm);
        }

        [HttpPost]
        public ActionResult UpdatePerm(int permId,string permName, string desc, string status)
        {
            Permissions Perm = db.Permissions.Find(permId);
            Perm.PermissionName = permName;
            Perm.Desc = desc;
            Perm.Status = status;
            db.Entry(Perm).State = EntityState.Modified;
            db.SaveChanges();
            var perm = db.Permissions.OrderBy(p => p.PermissionName).ToList();
            return Json(new { perm = perm }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeletePerm(int permId)
        {
            Permissions permission = db.Permissions.Find(permId);
            db.Permissions.Remove(permission);
            db.SaveChanges();
            var perm = db.Permissions.OrderBy(p => p.PermissionName).ToList();
            return Json(new { perm = perm }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermissionRoles perm = db.PermissionRoles.Find(id);
            if (perm == null)
            {
                return HttpNotFound();
            }
            return View(perm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            string username = User.Identity.Name;
            PermissionRoles perm = db.PermissionRoles.Find(id);
            PermissionRoles perm1 = new PermissionRoles();
            db.PermissionRoles.Remove(perm);
            db.SaveChanges();
            auditTransact.CreateAuditTrail("Delete", id, "Role Permission", perm, perm1, username);
            TempData["message"] = "Role Permission successfully deleted!";
            return RedirectToAction("Index");
        }
    }
}
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
    [Authorize]
    public class BusinessProcesController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();

        // GET: BusinessProces
        public ActionResult Index()
        {
            //var businessProcess = db.BusinessProcess.Include(b => b.Walktrough);
            return View();
        }

        // GET: BusinessProces/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BusinessProces businessProces = await db.BusinessProcess.FindAsync(id);
            if (businessProces == null)
            {
                return HttpNotFound();
            }
            return View(businessProces);
        }

        // GET: BusinessProces/Create
        public ActionResult Create()
        {
            //ViewBag.WalktroughID = new SelectList(db.Walktroughs, "WalktroughID", "Remarks");
            return View();
        }

        // POST: BusinessProces/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BusinessProcesID,BPMID,DocumentNo,DocumentName,FolderName")] BusinessProces businessProces)
        {
            if (ModelState.IsValid)
            {
                db.BusinessProcess.Add(businessProces);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            //ViewBag.WalktroughID = new SelectList(db.Walktroughs, "WalktroughID", "Remarks", businessProces.WalktroughID);
            return View(businessProces);
        }

        // GET: BusinessProces/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BusinessProces businessProces = await db.BusinessProcess.FindAsync(id);
            if (businessProces == null)
            {
                return HttpNotFound();
            }
            //ViewBag.WalktroughID = new SelectList(db.Walktroughs, "WalktroughID", "Remarks", businessProces.WalktroughID);
            return View(businessProces);
        }

        // POST: BusinessProces/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BusinessProcesID,BPMID,DocumentNo,DocumentName,FolderName")] BusinessProces businessProces)
        {
            if (ModelState.IsValid)
            {
                db.Entry(businessProces).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //ViewBag.WalktroughID = new SelectList(db.Walktroughs, "WalktroughID", "Remarks", businessProces.WalktroughID);
            return View(businessProces);
        }

        // GET: BusinessProces/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BusinessProces businessProces = await db.BusinessProcess.FindAsync(id);
            if (businessProces == null)
            {
                return HttpNotFound();
            }
            return View(businessProces);
        }

        // POST: BusinessProces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BusinessProces businessProces = await db.BusinessProcess.FindAsync(id);
            db.BusinessProcess.Remove(businessProces);
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
    }
}

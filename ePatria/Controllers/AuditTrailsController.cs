using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KellermanSoftware.CompareNetObjects;
using ePatria.Models;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ePatria.Controllers
{
    [Authorize]
    public class AuditTrailsController : Controller
    {
        public ActionResult Index()
        {
            return View(GetAllAudit());
        }
        private ePatriaDefault db = new ePatriaDefault();
        public void CreateAuditTrail(string Action, int KeyFieldID, string keyValue, Object OldValue, Object NewValue, string username)
        {   
            CompareLogic compLogic = new CompareLogic();
            compLogic.Config.MaxDifferences = 99;
            ComparisonResult compResult = compLogic.Compare(OldValue, NewValue);
            List<AuditTrailsTemp.AuditDelta> deltaList = new List<AuditTrailsTemp.AuditDelta>();
            foreach (var change in compResult.Differences)
            {
                AuditTrailsTemp.AuditDelta delta = new AuditTrailsTemp.AuditDelta();
                if (change.PropertyName.Substring(0, 1) == ".")
                    delta.FieldName = change.PropertyName.Substring(1, change.PropertyName.Length - 1);
                delta.ValueBefore = change.Object1Value;
                delta.ValueAfter = change.Object2Value;
                deltaList.Add(delta);
            }

            AuditTrails audit = new AuditTrails();
            audit.AuditAction = Action;
            audit.DataModel = this.GetType().Name;
            audit.DateTimeStamp = DateTime.Now;
            audit.KeyFieldID = KeyFieldID;
            audit.Desc = keyValue;
            audit.ValueBefore = JsonConvert.SerializeObject(OldValue);
            audit.ValueAfter = JsonConvert.SerializeObject(NewValue);
            audit.Changes = JsonConvert.SerializeObject(deltaList);
            audit.Username = username;
            db.AuditTrails.Add(audit);
            db.SaveChanges();
        }

        public List<AuditTrailsTemp.AuditChange> GetAudit(int ID)
        {
            List<AuditTrailsTemp.AuditChange> result = new List<AuditTrailsTemp.AuditChange>();
            var AuditTrail = db.AuditTrails.Where(s => s.KeyFieldID == ID).OrderByDescending(s => s.DateTimeStamp);
            var serializer = new XmlSerializer(typeof(AuditTrailsTemp.AuditDelta));
            foreach (var record in AuditTrail)
            {
                AuditTrailsTemp.AuditChange Change = new AuditTrailsTemp.AuditChange();
                Change.DateTimeStamp = record.DateTimeStamp.ToString();
                Change.AuditActionTypeName = record.AuditAction;
                List<AuditTrailsTemp.AuditDelta> delta = new List<AuditTrailsTemp.AuditDelta>();
                delta = JsonConvert.DeserializeObject<List<AuditTrailsTemp.AuditDelta>>(record.Changes);
                Change.Changes.AddRange(delta);
                result.Add(Change);
            }
            return result;
        }

        public List<AuditTrailsTemp.AuditChange> GetAllAudit()
        {
            List<AuditTrailsTemp.AuditChange> result = new List<AuditTrailsTemp.AuditChange>();
            var AuditTrail = db.AuditTrails;
            var serializer = new XmlSerializer(typeof(AuditTrailsTemp.AuditDelta));
            foreach (var record in AuditTrail)
            {
                AuditTrailsTemp.AuditChange Change = new AuditTrailsTemp.AuditChange();
                Change.DateTimeStamp = record.DateTimeStamp.ToString();
                Change.AuditActionTypeName = record.AuditAction + " " + record.Desc;
                Change.ValueId = record.KeyFieldID;
                Change.Username = record.Username;
                List<AuditTrailsTemp.AuditDelta> delta = new List<AuditTrailsTemp.AuditDelta>();
                delta = JsonConvert.DeserializeObject<List<AuditTrailsTemp.AuditDelta>>(record.Changes);
                Change.Changes.AddRange(delta);
                result.Add(Change);
            }
            return result.OrderByDescending(p => Convert.ToDateTime(p.DateTimeStamp)).ToList();
        }

    }
}
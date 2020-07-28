using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ePatria.Models
{
    public class AuditPlanningMemorandumServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<AuditPlanningMemorandum> GetAuditPlanningMemorandum()
        {
            return entities.AuditPlanningMemorandums.ToList();
        }

        public IEnumerable<AuditPlanningMemorandum> GetAuditPlanningMemorandumPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.AuditPlanningMemorandums
                .OrderBy(m => m.NomerAPM)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllAuditPlanningMemorandum()
        {
            return entities.AuditPlanningMemorandums.Count();
        }


        public AuditPlanningMemorandum GetAuditPlanningMemorandumDetail(int mCustID)
        {
            return entities.AuditPlanningMemorandums.Where(m => m.AuditPlanningMemorandumID == mCustID).FirstOrDefault();
        }

        public bool AuditPlanningMemorandum(AuditPlanningMemorandum org)
        {
            try
            {
                entities.AuditPlanningMemorandums.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateAuditPlanningMemorandum(AuditPlanningMemorandum org)
        {
            try
            {
                AuditPlanningMemorandum data = entities.AuditPlanningMemorandums.Where(m => m.AuditPlanningMemorandumID == org.AuditPlanningMemorandumID).FirstOrDefault();

                data.NomerAPM = org.NomerAPM;
                data.PreliminaryID = org.PreliminaryID;
                data.Date_Start = org.Date_Start;
                data.Date_End = org.Date_End;
                data.Status = org.Status;
                data.ActivityID = org.ActivityID;
                data.TujuanAudit = org.TujuanAudit;
                data.RuangLingkupAudit = org.RuangLingkupAudit;
                data.MetodologiAudit = org.MetodologiAudit;
                data.DataDanDokumen = org.DataDanDokumen;
                data.EntryMeetingDateStart = org.EntryMeetingDateStart;
                //data.EntryMeetingDateEnd = org.EntryMeetingDateEnd;
                data.WalktroughDateStart = org.WalktroughDateStart;
                data.WalktroughDateEnd = org.WalktroughDateEnd;
                data.FieldWorkDateStart = org.FieldWorkDateStart;
                data.FieldWorkDateEnd = org.FieldWorkDateEnd;
                data.ExitMeetingDateStart = org.ExitMeetingDateStart;
                //data.ExitMeetingDateEnd = org.ExitMeetingDateEnd;
                data.LHADateStart = org.LHADateStart;
                //data.LHADateEnd = org.LHADateEnd;
                data.PICID = org.PICID;
                data.SupervisorID = org.SupervisorID;
                data.TeamLeaderID = org.TeamLeaderID;
                data.MemberID = org.MemberID;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteAuditPlanningMemorandum(int mCustID)
        {
            try
            {
                AuditPlanningMemorandum data = entities.AuditPlanningMemorandums.Where(m => m.AuditPlanningMemorandumID == mCustID).FirstOrDefault();
                entities.AuditPlanningMemorandums.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class AuditPlanningMemorandum
    {
        public int AuditPlanningMemorandumID { get; set; }
        public string NomerAPM { get; set; }
        public int PreliminaryID { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public string Status { get; set; }
        public int ActivityID { get; set; }
        public string ActualEngagement { get; set; }
        public string TujuanAudit { get; set; }
        public string RuangLingkupAudit { get; set; }
        public string MetodologiAudit { get; set; }
        public string DataDanDokumen { get; set; }
        public string Kesimpulan { get; set; }
        public DateTime EntryMeetingDateStart { get; set; }
        //public DateTime EntryMeetingDateEnd { get; set; }
        public DateTime WalktroughDateStart { get; set; }
        public DateTime WalktroughDateEnd { get; set; }
        public DateTime FieldWorkDateStart { get; set; }
        public DateTime FieldWorkDateEnd { get; set; }
        public DateTime ExitMeetingDateStart { get; set; }
        //public DateTime ExitMeetingDateEnd { get; set; }
        public DateTime LHADateStart { get; set; }
        //public DateTime LHADateEnd { get; set; }

        public string PICID { get; set; }
        public string SupervisorID { get; set; }
        public string TeamLeaderID { get; set; }
        public string MemberID { get; set; }
        public int? ReviewRelationMasterID { get; set; }

        public virtual Preliminary Preliminary { get; set; }
        public virtual Activity Activity { get; set; }
        public virtual ReviewRelationMaster ReviewRelationMaster { get; set; }
        private readonly ePatriaDefault entities = new ePatriaDefault();
        public string getNoPekEmpByName(string empName)
        {
            string noPek = entities.Employees.Where(p => p.Name == empName).FirstOrDefault().NoPEK;
            return noPek;
        }
    }

    public class PagedAuditPlanningMemorandumModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<AuditPlanningMemorandum> AuditPlanningMemorandum { get; set; }
        public int PageSize { get; set; }


        public int AuditPlanningMemorandumID { get; set; }
        public string NomerAPM { get; set; }
        public int PreliminaryID { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public string Status { get; set; }
        public int ActivityID { get; set; }
        public string TujuanAudit { get; set; }
        public string RuangLingkupAudit { get; set; }
        public string MetodologiAudit { get; set; }
        public string DataDanDokumen { get; set; }
        public DateTime EntryMeetingDateStart { get; set; }
        public DateTime EntryMeetingDateEnd { get; set; }
        public DateTime WalktroughDateStart { get; set; }
        public DateTime WalktroughDateEnd { get; set; }
        public DateTime FieldWorkDateStart { get; set; }
        public DateTime FieldWorkDateEnd { get; set; }
        public DateTime ExitMeetingDateStart { get; set; }
        public DateTime ExitMeetingDateEnd { get; set; }
        public DateTime LHADateStart { get; set; }
        public DateTime LHADateEnd { get; set; }
        public int PICID { get; set; }
        public int SupervisorID { get; set; }
        public int TeamLeaderID { get; set; }
        public int MemberID { get; set; }
        public int ReviewMasterID { get; set; }
        public int ReviewRelationMasterID { get; set; }

        public virtual Preliminary Preliminary { get; set; }
        public virtual Activity Activity { get; set; }
        public virtual ReviewRelationMaster ReviewRelationMaster { get; set; }

    }
}
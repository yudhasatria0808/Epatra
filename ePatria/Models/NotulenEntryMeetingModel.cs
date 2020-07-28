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
    public class NotulenEntryMeetingServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<NotulenEntryMeeting> GetNotulenEntryMeeting()
        {
            return entities.NotulenEntryMeetings.ToList();
        }

        public IEnumerable<NotulenEntryMeeting> GetNotulenEntryMeetingPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.NotulenEntryMeetings
                .OrderBy(m => m.Tujuan)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllNotulenEntryMeeting()
        {
            return entities.NotulenEntryMeetings.Count();
        }


        public NotulenEntryMeeting GetNotulenEntryMeetingDetail(int mCustID)
        {
            return entities.NotulenEntryMeetings.Where(m => m.NotulenEntryMeetingID == mCustID).FirstOrDefault();
        }

        public bool AddNotulenEntryMeeting(NotulenEntryMeeting org)
        {
            try
            {
                entities.NotulenEntryMeetings.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateNotulenEntryMeeting(NotulenEntryMeeting org)
        {
            try
            {
                NotulenEntryMeeting data = entities.NotulenEntryMeetings.Where(m => m.NotulenEntryMeetingID == org.NotulenEntryMeetingID).FirstOrDefault();

                data.AuditPlanningMemorandumID = org.AuditPlanningMemorandumID;
                data.Tujuan = org.Tujuan;
                data.TimeConsumableStartDate = org.TimeConsumableStartDate;
                data.TimeConsumableEndDate = org.TimeConsumableEndDate;
                data.Place = org.Place;
                data.EmployeeChairmanID = org.EmployeeChairmanID;
                data.EmployeeModeratorID = org.EmployeeModeratorID;
                data.EmployeeReporterID = org.EmployeeReporterID;
                data.EmployeeMemberID = org.EmployeeMemberID;
                data.Opening = org.Opening;
                data.ExposurePlan = org.ExposurePlan;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteNotulenEntryMeeting(int mCustID)
        {
            try
            {
                NotulenEntryMeeting data = entities.NotulenEntryMeetings.Where(m => m.NotulenEntryMeetingID == mCustID).FirstOrDefault();
                entities.NotulenEntryMeetings.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class NotulenEntryMeeting
    {
        public int NotulenEntryMeetingID { get; set; }
        public int? AuditPlanningMemorandumID { get; set; }
        public string Tujuan { get; set; }
        public DateTime TimeConsumableStartDate { get; set; }
        public DateTime TimeConsumableEndDate { get; set; }
        public string Place { get; set; }
        public int EmployeeChairmanID { get; set; }
        public int EmployeeModeratorID { get; set; }
        public int EmployeeReporterID { get; set; }
        public int EmployeeMemberID { get; set; }
        [DataType(DataType.Text)]
        public string Opening { get; set; }
        [DataType(DataType.Text)]
        public string ExposurePlan { get; set; }
        public int LetterOfCommandID { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }
        public virtual AuditPlanningMemorandum AuditPlanningMemorandum { get; set; }
    }


    public class PagedNotulenEntryMeetingModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<NotulenEntryMeeting> NotulenEntryMeeting { get; set; }
        public int PageSize { get; set; }


        public int NotulenEntryMeetingID { get; set; }
        public int AuditPlanningMemorandumID { get; set; }
        public string Tujuan { get; set; }
        public string Place { get; set; }
        public DateTime TimeConsumableStartDate { get; set; }
        public DateTime TimeConsumableEndDate { get; set; }
        public int EmployeeChairmanID { get; set; }
        public int EmployeeModeratorID { get; set; }
        public int EmployeeReporterID { get; set; }
        public int EmployeeMemberID { get; set; }
        public string Opening { get; set; }
        public string ExposurePlan { get; set; }
        public virtual AuditPlanningMemorandum AuditPlanningMemorandum { get; set; }
        
    }
}

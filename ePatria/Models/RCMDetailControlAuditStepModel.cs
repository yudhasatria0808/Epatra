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
    public class RCMDetailControlAuditStepServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<RCMDetailControlAuditStep> GetRCMDetailControlAuditStep()
        {
            return entities.RCMDetailControlAuditSteps.ToList();
        }

        public IEnumerable<RCMDetailControlAuditStep> GetRCMDetailControlAuditStepPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.RCMDetailControlAuditSteps
                .OrderBy(m => m.RCMDetailControlAuditStepID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllRCMDetailControlAuditStep()
        {
            return entities.RCMDetailControlAuditSteps.Count();
        }


        public RCMDetailControlAuditStep GetRCMDetailControlAuditStepDetail(int mCustID)
        {
            return entities.RCMDetailControlAuditSteps.Where(m => m.RCMDetailControlAuditStepID == mCustID).FirstOrDefault();
        }

        public bool RCMDetailControlAuditStep(RCMDetailControlAuditStep org)
        {
            try
            {
                entities.RCMDetailControlAuditSteps.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateRCMDetailControlAuditStep(RCMDetailControlAuditStep org)
        {
            try
            {
                RCMDetailControlAuditStep data = entities.RCMDetailControlAuditSteps.Where(m => m.RCMDetailControlAuditStepID == org.RCMDetailControlAuditStepID).FirstOrDefault();

                data.RCMDetailRiskControlID = org.RCMDetailRiskControlID;
                data.AuditStepName = org.AuditStepName;
                data.Status = org.Status;
                data.WorkDoneDescription = org.WorkDoneDescription;
                data.WorkDoneResult = org.WorkDoneResult;


                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteRCMDetailControlAuditStep(int mCustID)
        {
            try
            {
                RCMDetailControlAuditStep data = entities.RCMDetailControlAuditSteps.Where(m => m.RCMDetailControlAuditStepID == mCustID).FirstOrDefault();
                entities.RCMDetailControlAuditSteps.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class RCMDetailControlAuditStep
    {
        public int RCMDetailControlAuditStepID { get; set; }
        public int RCMDetailRiskControlID { get; set; }
        public string AuditStepName { get; set; }       
        public string Status { get; set; }
        public string WorkDoneDescription { get; set; }
        public string WorkDoneResult { get; set; }


        public virtual RCMDetailRiskControl RCMDetailRiskControl { get; set; }
    
    }

    public class PagedRCMDetailControlAuditStepModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<RCMDetailControlAuditStep> RCMDetailControlAuditStep { get; set; }
        public int PageSize { get; set; }


        public int RCMDetailControlAuditStepID { get; set; }
        public int RCMDetailRiskControlID { get; set; }
        public string AuditStepName { get; set; }
        public string Status { get; set; }
        public string WorkDoneDescription { get; set; }
        public string WorkDoneResult { get; set; }
        public int AttachmentMasterID { get; set; }
        public int ReviewMasterID { get; set; }
        public virtual RCMDetailRiskControl RCMDetailRiskControl { get; set; }
    
    }
}
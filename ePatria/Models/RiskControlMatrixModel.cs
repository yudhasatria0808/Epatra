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
    public class RiskControlMatrixServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<RiskControlMatrix> GetRiskControlMatrix()
        {
            return entities.RiskControlMatrixs.ToList();
        }

        public IEnumerable<RiskControlMatrix> GetRiskControlMatrixPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.RiskControlMatrixs
                .OrderBy(m => m.RiskControlMatrixID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllRiskControlMatrix()
        {
            return entities.RiskControlMatrixs.Count();
        }


        public RiskControlMatrix GetRiskControlMatrixDetail(int mCustID)
        {
            return entities.RiskControlMatrixs.Where(m => m.RiskControlMatrixID == mCustID).FirstOrDefault();
        }

        public bool RiskControlMatrix(RiskControlMatrix org)
        {
            try
            {
                entities.RiskControlMatrixs.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateRiskControlMatrix(RiskControlMatrix org)
        {
            try
            {
                RiskControlMatrix data = entities.RiskControlMatrixs.Where(m => m.RiskControlMatrixID == org.RiskControlMatrixID).FirstOrDefault();

                data.BusinessProcesID = org.BusinessProcesID;
                data.SubBusinessProcess = org.SubBusinessProcess;
                data.Objectives = org.Objectives;
               

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteRiskControlMatrix(int mCustID)
        {
            try
            {
                RiskControlMatrix data = entities.RiskControlMatrixs.Where(m => m.RiskControlMatrixID == mCustID).FirstOrDefault();
                entities.RiskControlMatrixs.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    
    public class RiskControlMatrix
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();
        public int RiskControlMatrixID { get; set; }
        public int BusinessProcesID { get; set; }
        public string SubBusinessProcess { get; set; }
        public string Objectives { get; set; }
        public int WalktroughID { get; set; }
        public string Status { get; set; }
        public virtual BusinessProces BusinessProces { get; set; }
        public virtual Walktrough Walktrough { get; set; }
        public string getBPMName(int bpmId)
        {
            string bpmName = entities.BPMs.Find(bpmId).Name;
            return bpmName;
        }
        public List<RCMDetailRisk> getRiskByRCMId(int rcmId)
        {
            List<RCMDetailRisk> risk = entities.RCMDetailRisks.Where(p => p.RiskControlMatrixID == rcmId).ToList();
            return risk;
        }
        public List<RCMDetailRiskControl> getControlByRiskId(int riskId)
        {
            List<RCMDetailRiskControl> control = entities.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).ToList();
            return control;
        }
        public List<RCMDetailControlAuditStep> getAuditStepByControlId(int controlId)
        {
            List<RCMDetailControlAuditStep> audit = entities.RCMDetailControlAuditSteps.Where(p => p.RCMDetailRiskControlID == controlId).ToList();
            return audit;
        }
        public List<RCMDetailRiskControlDetail> getWorkDoneDescriptionByControlId(int controlId)
        {
            List<RCMDetailRiskControlDetail> wdd = entities.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID == controlId).ToList();
            return wdd;
        }
        public List<RCMDetailRiskControlDetail> getWorkDoneResultByControlId(int controlId)
        {
            List<RCMDetailRiskControlDetail> wdr = entities.RCMDetailRiskControlDetail.Where(p => p.RCMDetailRiskControlID == controlId).ToList();
            return wdr;
        }
        public List<RCMDetailRiskControlIssue> getIssueTitleByControlId(int controlId)
        {
            List<RCMDetailRiskControlIssue> title = entities.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID == controlId).ToList();
            return title;
        }
        public List<RCMDetailRiskControl> getControlStatusByRiskId(int riskId)
        {
            List<RCMDetailRiskControl> auditresult = entities.RCMDetailRiskControls.Where(p => p.RCMDetailRiskID == riskId).ToList();
            return auditresult;
        }
        public List<RCMDetailRiskControlIssue> getIssuePICIDByControlId(int controlId)
        {
            List<RCMDetailRiskControlIssue> auditor = entities.RCMDetailRiskControlIssue.Where(p => p.RCMDetailRiskControlID == controlId).ToList();
            return auditor;
        }
    }

    public class PagedRiskControlMatrixModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<RiskControlMatrix> RiskControlMatrix { get; set; }
        public int PageSize { get; set; }


        public int RiskControlMatrixID { get; set; }
        public int BusinessProcesID { get; set; }
        public string SubBusinessProcess { get; set; }
        public string Objectives { get; set; }
        
        public virtual BusinessProces BusinessProces { get; set; }

    }
}
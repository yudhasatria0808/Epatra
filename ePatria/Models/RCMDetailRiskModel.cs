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
    public class RCMDetailRiskServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<RCMDetailRisk> GetRCMDetailRisk()
        {
            return entities.RCMDetailRisks.ToList();
        }

        public IEnumerable<RCMDetailRisk> GetRCMDetailRiskPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.RCMDetailRisks
                .OrderBy(m => m.RCMDetailRiskID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllRCMDetailRisk()
        {
            return entities.RCMDetailRisks.Count();
        }


        public RCMDetailRisk GetRCMDetailRiskDetail(int mCustID)
        {
            return entities.RCMDetailRisks.Where(m => m.RCMDetailRiskID == mCustID).FirstOrDefault();
        }

        public bool RCMDetailRisk(RCMDetailRisk org)
        {
            try
            {
                entities.RCMDetailRisks.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateRCMDetailRisk(RCMDetailRisk org)
        {
            try
            {
                RCMDetailRisk data = entities.RCMDetailRisks.Where(m => m.RCMDetailRiskID == org.RCMDetailRiskID).FirstOrDefault();

                data.RiskControlMatrixID = org.RiskControlMatrixID;
                data.RiskName = org.RiskName;
                data.Status = org.Status;


                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteRCMDetailRisk(int mCustID)
        {
            try
            {
                RCMDetailRisk data = entities.RCMDetailRisks.Where(m => m.RCMDetailRiskID == mCustID).FirstOrDefault();
                entities.RCMDetailRisks.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class RCMDetailRisk
    {
        public int RCMDetailRiskID { get; set; }
        public int RiskControlMatrixID { get; set; }
        public string RiskName { get; set; }
        public string Status { get; set; }

        public virtual RiskControlMatrix RiskControlMatrix { get; set; }
      
    }

    public class PagedRCMDetailRiskModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<RCMDetailRisk> RCMDetailRisk { get; set; }
        public int PageSize { get; set; }


        public int RCMDetailRiskID { get; set; }
        public int RiskControlMatrixID { get; set; }
        public string RiskName { get; set; }
        public string Status { get; set; }
        public virtual RiskControlMatrix RiskControlMatrix { get; set; }
 

    }
}
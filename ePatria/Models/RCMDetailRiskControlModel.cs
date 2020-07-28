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
    public class RCMDetailRiskControlServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<RCMDetailRiskControl> GetRCMDetailRiskControl()
        {
            return entities.RCMDetailRiskControls.ToList();
        }

        public IEnumerable<RCMDetailRiskControl> GetRCMDetailRiskControlPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.RCMDetailRiskControls
                .OrderBy(m => m.RCMDetailRiskControlID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllRCMDetailRiskControl()
        {
            return entities.RCMDetailRiskControls.Count();
        }


        public RCMDetailRiskControl GetRCMDetailRiskControlDetail(int mCustID)
        {
            return entities.RCMDetailRiskControls.Where(m => m.RCMDetailRiskControlID == mCustID).FirstOrDefault();
        }

        public bool RCMDetailRiskControl(RCMDetailRiskControl org)
        {
            try
            {
                entities.RCMDetailRiskControls.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateRCMDetailRiskControl(RCMDetailRiskControl org)
        {
            try
            {
                RCMDetailRiskControl data = entities.RCMDetailRiskControls.Where(m => m.RCMDetailRiskControlID == org.RCMDetailRiskControlID).FirstOrDefault();

                data.RCMDetailRiskID = org.RCMDetailRiskID;
                data.ControlName = org.ControlName;
                data.Status = org.Status;


                data.RefNo = org.RefNo;
                data.Title = org.Title;

                data.Createria = org.Createria;
                data.Impact = org.Impact;
                data.ImpactValue = org.ImpactValue;
                data.Cause = org.Cause;
                data.Recommendation = org.Recommendation;
                data.ManagementResponse = org.ManagementResponse;
                data.PIC = org.PIC;
                data.FindingType = org.FindingType;
                data.DueDate = org.DueDate;
                data.CaseCloseBool = org.CaseCloseBool;
                data.CloseDate = org.CloseDate;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteRCMDetailRiskControl(int mCustID)
        {
            try
            {
                RCMDetailRiskControl data = entities.RCMDetailRiskControls.Where(m => m.RCMDetailRiskControlID == mCustID).FirstOrDefault();
                entities.RCMDetailRiskControls.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class RCMDetailRiskControl
    {
        public int RCMDetailRiskControlID { get; set; }
        public int RCMDetailRiskID { get; set; }
        public string ControlName { get; set; }
        public string Status { get; set; }
        public int ReviewMasterID { get; set; }
        public string RefNo { get; set; }
        public string Title { get; set; }
        [DataType(DataType.Text)]
        public string Createria { get; set; }
        [DataType(DataType.Text)]
        public string Impact { get; set; }
        [DataType(DataType.Text)]
        public string ImpactValue { get; set; }
        [DataType(DataType.Text)]
        public string Cause { get; set; }
        [DataType(DataType.Text)]
        public string Recommendation { get; set; }
        [DataType(DataType.Text)]
        public string ManagementResponse { get; set; }
        public string PIC { get; set; }
        public string FindingType { get; set; }
        public DateTime DueDate { get; set; }
        public string CaseCloseBool { get; set; }
        public DateTime CloseDate { get; set; }
        public string ReviewStatus { get; set; }
        public string keterangan { get; set; }
        public virtual RCMDetailRisk RCMDetailRisk { get; set; }
    }

    public class PagedRCMDetailRiskControlModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<RCMDetailRiskControl> RCMDetailRiskControl { get; set; }
        public int PageSize { get; set; }


        public int RCMDetailRiskControlID { get; set; }
        public int RCMDetailRiskID { get; set; }
        public string ControlName { get; set; }
        public string Status { get; set; }
        public int ReviewMasterID { get; set; }
        public string RefNo { get; set; }
        public string Title { get; set; }
        public string Createria { get; set; }
        public string Impact { get; set; }
        public string ImpactValue { get; set; }
        public string Cause { get; set; }
        public string Recommendation { get; set; }
        public string ManagementResponse { get; set; }
        public string PIC { get; set; }
        public string FindingType { get; set; }
        public DateTime DueDate { get; set; }
        public string CaseCloseBool { get; set; }
        public DateTime CloseDate { get; set; }

        public virtual RCMDetailRisk RCMDetailRisk { get; set; }

    }
}
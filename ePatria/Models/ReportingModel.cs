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
    public class ReportingServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<Reporting> GetReporting()
        {
            return entities.Reportings.ToList();
        }

        public IEnumerable<Reporting> GetReportingPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Reportings
                .OrderBy(m => m.ReportingID)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllReporting()
        {
            return entities.Reportings.Count();
        }


        public Reporting GetReportingDetail(int mCustID)
        {
            return entities.Reportings.Where(m => m.ReportingID == mCustID).FirstOrDefault();
        }

        public bool AddReporting(Reporting org)
        {
            try
            {
                entities.Reportings.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateReporting(Reporting org)
        {
            try
            {
                Reporting data = entities.Reportings.Where(m => m.ReportingID == org.ReportingID).FirstOrDefault();

                data.LetterOfCommandID = org.LetterOfCommandID;
                data.NomorLaporan = org.NomorLaporan;
                data.MemoDescription = org.MemoDescription;
                data.SummaryDescription = org.SummaryDescription;
                data.Bab01Title = org.Bab01Title;
                data.Bab01SubTitle = org.Bab01SubTitle;
                data.Bab01Description = org.Bab01Description;
                data.Bab02Title = org.Bab02Title;
                data.Bab02SubTitle = org.Bab02SubTitle;
                data.Bab02Description = org.Bab02Description;
                


                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteReporting(int mCustID)
        {
            try
            {
                Reporting data = entities.Reportings.Where(m => m.ReportingID == mCustID).FirstOrDefault();
                entities.Reportings.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class Reporting
    {
        public Reporting()
        {
            this.ReportingBabModel = new HashSet<ReportingBabModel>();
        }
        public int ReportingID { get; set; }
        public int LetterOfCommandID { get; set; }
        public string NomorLaporan { get; set; }
        public string MemoDescription { get; set; }
        public string SummaryDescription { get; set; }
        public string Bab01Title { get; set; }
        public string Bab01SubTitle { get; set; }
        public string Bab01Description { get; set; }
        public string Bab02Title { get; set; }
        public string Bab02SubTitle { get; set; }
        public string Bab02Description { get; set; }
        public string Fact { get; set; }
        public string Criteria { get; set; }
        public string Impact { get; set; }
        public string impactValue { get; set; }
        public string Cause { get; set; }
        public string Recommendation { get; set; }
        public int? FieldWorkID { get; set; }
        public List<ReportingBabModel> ListBab { get; set; }

        public virtual LetterOfCommand LetterOfCommand { get; set; }
        public virtual FieldWork FieldWork { get; set; }
        public virtual ICollection<ReportingBabModel> ReportingBabModel { get; set; }
    }

    public class PagedReportingModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<Reporting> Reporting { get; set; }
        public int PageSize { get; set; }


        public int ReportingID { get; set; }
        public int LetterOfCommandID { get; set; }
        public string NomorLaporan { get; set; }
        [DataType(DataType.Text)]
        public string MemoDescription { get; set; }
        [DataType(DataType.Text)]
        public string SummaryDescription { get; set; }
        public string Bab01Title { get; set; }
        public string Bab01SubTitle { get; set; }
        [DataType(DataType.Text)]
        public string Bab01Description { get; set; }
        public string Bab02Title { get; set; }
        public string Bab02SubTitle { get; set; }
        [DataType(DataType.Text)]
        public string Bab02Description { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }
    }
}
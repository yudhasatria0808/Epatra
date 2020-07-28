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
    public class BusinessProcesServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<BusinessProces> GetBusinessProces()
        {
            return entities.BusinessProcess.ToList();
        }

        public IEnumerable<BusinessProces> GetBusinessProcesPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.BusinessProcess
                .OrderBy(m => m.DocumentName)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllBusinessProces()
        {
            return entities.BusinessProcess.Count();
        }


        public BusinessProces GetBusinessProcesDetail(int mCustID)
        {
            return entities.BusinessProcess.Where(m => m.BusinessProcesID == mCustID).FirstOrDefault();
        }

        public bool AddBusinessProces(BusinessProces org)
        {
            try
            {
                entities.BusinessProcess.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateBusinessProces(BusinessProces org)
        {
            try
            {
                BusinessProces data = entities.BusinessProcess.Where(m => m.BusinessProcesID == org.BusinessProcesID).FirstOrDefault();

                data.BPMID = org.BPMID;
                data.DocumentNo = org.DocumentNo;
                data.DocumentName = org.DocumentName;
                data.FolderName = org.FolderName;
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteBusinessProces(int mCustID)
        {
            try
            {
                BusinessProces data = entities.BusinessProcess.Where(m => m.BusinessProcesID == mCustID).FirstOrDefault();
                entities.BusinessProcess.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public class BusinessProces
    {
            public int BusinessProcesID { get; set; }
            public int BPMID { get; set; }
            public int? WalktroughID { get; set; }
            public string DocumentNo { get; set; }
            public string DocumentName { get; set; }
            public string FolderName { get; set; }

            public virtual Walktrough Walktrough { get; set; }
            public virtual BPM BPM { get; set; }
        
    }
    public class PagedBusinessProcesModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<BusinessProces> BusinessProces { get; set; }
        public int PageSize { get; set; }

        public int BusinessProcesID { get; set; }
        public int WalktroughID { get; set; }
        public string DocumentNo { get; set; }
        public string DocumentName { get; set; }
        public string FolderName { get; set; }

        public virtual Walktrough Walktrough { get; set; }
    }
}
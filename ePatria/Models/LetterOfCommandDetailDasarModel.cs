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
    //public class LetterOfCommandDetailDasarServices
    //{
    //    private readonly ePatriaDefault entities = new ePatriaDefault();

    //    public void Dispose()
    //    {
    //        entities.Dispose();
    //    }
    //    public IEnumerable<LetterOfCommandDetailDasar> GetLetterOfCommandDetailDasar()
    //    {
    //        return entities.LetterOfCommandDetailDasars.ToList();
    //    }

    //    public IEnumerable<LetterOfCommandDetailDasar> GetLetterOfCommandDetailDasarPage(int pageNumber, int pageSize, string searchCriteria)
    //    {
    //        if (pageNumber < 1)
    //            pageNumber = 1;

    //        return entities.LetterOfCommandDetailDasars
    //            .OrderBy(m => m.Dasar)
    //          .Skip((pageNumber - 1) * pageSize)
    //          .Take(pageSize)
    //          .ToList();
    //    }
    //    public int CountAllLetterOfCommandDetailDasar()
    //    {
    //        return entities.LetterOfCommandDetailDasars.Count();
    //    }


    //    public LetterOfCommandDetailDasar GetLetterOfCommandDetailDasarDetail(int mCustID)
    //    {
    //        return entities.LetterOfCommandDetailDasars.Where(m => m.ID == mCustID).FirstOrDefault();
    //    }

    //    public bool AddLetterOfCommandDetailDasar(LetterOfCommandDetailDasar org)
    //    {
    //        try
    //        {
    //            entities.LetterOfCommandDetailDasars.Add(org);
    //            entities.SaveChanges();
    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //    }

    //    public bool UpdateLetterOfCommandDetailDasar(LetterOfCommandDetailDasar org)
    //    {
    //        try
    //        {
    //            LetterOfCommandDetailDasar data = entities.LetterOfCommandDetailDasars.Where(m => m.ID == org.ID).FirstOrDefault();

    //            data.LetterOfCommandID = org.LetterOfCommandID;
    //            data.Dasar = org.Dasar;

    //            entities.SaveChanges();
    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //    }

    //    public bool DeleteLetterOfCommandDetailDasar(int mCustID)
    //    {
    //        try
    //        {
    //            LetterOfCommandDetailDasar data = entities.LetterOfCommandDetailDasars.Where(m => m.ID == mCustID).FirstOrDefault();
    //            entities.LetterOfCommandDetailDasars.Remove(data);
    //            entities.SaveChanges();
    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //    }
    //}

    public class LetterOfCommandDetailDasar
    {
        public int ID { get; set; }
        public int LetterOfCommandID { get; set; }
        public string Dasar { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }

    }

  
}
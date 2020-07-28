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

    public class ActivityServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<Activity> GetActivity()
        {
            return entities.Activities.ToList();
        }

        public IEnumerable<Activity> GetActivityPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Activities
                .OrderBy(m => m.Name)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllActivity()
        {
            return entities.Activities.Count();
        }


        //For Edit Activity
        public Activity GetActivityDetail(int mCustID)
        {
            return entities.Activities.Where(m => m.ActivityID == mCustID).FirstOrDefault();
        }

        public bool AddActivity(Activity org)
        {
            try
            {
                entities.Activities.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateActivity(Activity org)
        {
            try
            {
                Activity data = entities.Activities.Where(m => m.ActivityID == org.ActivityID).FirstOrDefault();

                data.ActivityParentID = org.ActivityParentID;
                data.Name = org.Name;
                data.Status = org.Status;
                data.Tahun = org.Tahun;
                data.Description = org.Description;
                data.DepartementID = org.DepartementID;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteActivity(int mCustID)
        {
            try
            {
                Activity data = entities.Activities.Where(m => m.ActivityID == mCustID).FirstOrDefault();
                entities.Activities.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class PagedActivityModel
    {
        public int TotalRows { get; set; }
        public int ActivityID { get; set; }
        public int? ActivityParentID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Tahun { get; set; }
        public string Description { get; set; }
        public int? DepartementID { get; set; }

    }

    public class Activity
    {
        public int ActivityID { get; set; }
        public int? ActivityParentID { get; set; }
        [Required(ErrorMessage = "- Please Input Name -")]
        public string Name { get; set; }
        public string Status { get; set; }
        public string Tahun { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "- Select item -")]
        public int? DepartementID { get; set; }
        public string Status_Active { get; set; }

        //public IEnumerable<Organization> Organization { get; set; }


    }
}
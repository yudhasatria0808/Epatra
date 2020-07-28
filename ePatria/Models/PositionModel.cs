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


    public class PositionServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }

        public IEnumerable<Position> GetPosition()
        {
            return entities.Positions.ToList();
        }

        public IEnumerable<Position> GetPositionPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Positions
                .OrderBy(m => m.PositionName)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllPosition()
        {
            return entities.Positions.Count();
        }


        //For Edit Position
        public Position GetPositionDetail(int mCustID)
        {
            return entities.Positions.Where(m => m.PositionID == mCustID).FirstOrDefault();
        }

        public bool AddPosition(Position org)
        {
            try
            {
                entities.Positions.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdatePosition(Position org)
        {
            try
            {
                Position data = entities.Positions.Where(m => m.PositionID == org.PositionID).FirstOrDefault();

                data.PositionName = org.PositionName;
                data.JobDesc = org.JobDesc;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeletePosition(int mCustID)
        {
            try
            {
                Position data = entities.Positions.Where(m => m.PositionID == mCustID).FirstOrDefault();
                entities.Positions.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }


    public class PagedPositionModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<Position> Position { get; set; }
        public int PageSize { get; set; }

        public int PositionID { get; set; }
        public string PositionName { get; set; }
        public string JobDesc { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Preliminary> Preliminary { get; set; }

    }

    public class Position
    {

        public int PositionID { get; set; }
        [Required(ErrorMessage = "- Please Input Position Name -")]
        public string PositionName { get; set; }
        public string JobDesc { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Preliminary> Preliminary { get; set; }
    }
}
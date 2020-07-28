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
    public class OrganizationServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }

        public IEnumerable<Organization> GetOrganization()
        {
            return entities.Organizations.ToList();
        }

        public IEnumerable<Organization> GetOrganizationPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Organizations
                .OrderBy(m => m.Name)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllOrganization()
        {
            return entities.Organizations.Count();
        }


        //For Edit Organization
        public Organization GetOrganizationDetail(int mCustID)
        {
            return entities.Organizations.Where(m => m.OrganizationID == mCustID).FirstOrDefault();
        }

        public bool AddOrganization(Organization org)
        {
            try
            {
                entities.Organizations.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateOrganization(Organization org)
        {
            try
            {
                Organization data = entities.Organizations.Where(m => m.OrganizationID == org.OrganizationID).FirstOrDefault();

                data.OrganizationParentID = org.OrganizationParentID;
                data.Name = org.Name;
                data.Description = org.Description;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteOrganization(int mCustID)
        {
            try
            {
                Organization data = entities.Organizations.Where(m => m.OrganizationID == mCustID).FirstOrDefault();
                entities.Organizations.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class PagedOrganizationModel
    {
        public int TotalRows { get; set; }

        public IEnumerable<Position> Position { get; set; }
        public IEnumerable<Employee> Employee { get; set; }
        public int PageSize { get; set; }

        public int OrganizationID { get; set; }
        public int? OrganizationParentID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }

    public class Organization
    {

        public int OrganizationID { get; set; }
        public int? OrganizationParentID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }

}
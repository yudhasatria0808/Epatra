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

    public class EmployeeServices
    {
        private readonly ePatriaDefault entities = new ePatriaDefault();

        public void Dispose()
        {
            entities.Dispose();
        }
        public IEnumerable<Employee> GetEmployee()
        {
            return entities.Employees.ToList();
        }

        public IEnumerable<Employee> GetEmployeenPage(int pageNumber, int pageSize, string searchCriteria)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            return entities.Employees
                .OrderBy(m => m.Name)
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
              .ToList();
        }
        public int CountAllEmployee()
        {
            return entities.Employees.Count();
        }


        //For Edit Employee
        public Employee GetEmployeeDetail(int mCustID)
        {
            return entities.Employees.Where(m => m.EmployeeID == mCustID).FirstOrDefault();
        }

        public bool AddEmployee(Employee org)
        {
            try
            {
                entities.Employees.Add(org);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateEmployee(Employee org)
        {
            try
            {
                Employee data = entities.Employees.Where(m => m.EmployeeID == org.EmployeeID).FirstOrDefault();

                data.Type = org.Type;
                data.Name = org.Name;
                data.NoPEK = org.NoPEK;
                data.Email = org.Email;
                data.PhoneNumber = org.PhoneNumber;
                data.Status = org.Status;
                data.OrganizationID = org.OrganizationID;
                data.PositionID = org.PositionID;

                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteEmployee(int mCustID)
        {
            try
            {
                Employee data = entities.Employees.Where(m => m.EmployeeID == mCustID).FirstOrDefault();
                entities.Employees.Remove(data);
                entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    
    }

    public class Employee
    {
        public int EmployeeID { get; set; }
        [Required(ErrorMessage = "- Please Select Type -")]
        public string Type { get; set; }
        [Required(ErrorMessage = "- Please Input Name -")]
        public string Name { get; set; }
        [Required(ErrorMessage = "- Please Input NO. PEK -")]
        public string NoPEK { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "- Please Input Email -")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "- Please Select Status -")]
        public string Status { get; set; }
        [Required]
        public int OrganizationID { get; set; }
        [Required]
        public int PositionID { get; set; }
        public string UserName { get; set; }
        public virtual Organization Organizations { get; set; }
        public virtual Position Positions { get; set; }

    }

     public class PagedEmployeeModel
    {
        public int TotalRows { get; set; }
        public IEnumerable<Employee> Employee { get; set; }
        public int PageSize { get; set; }
        public int EmployeeID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string NoPEK { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }

        public int OrganizationID { get; set; }
        public int PositionID { get; set; }
    
        public virtual Organization Organizations { get; set; }
        public virtual Position Positions { get; set; }
    }
}
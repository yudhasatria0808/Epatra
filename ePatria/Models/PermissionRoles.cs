using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class PermissionRoles
    {
        [Key]
        public int PermissionRoleID { get; set; }
        public int permissionID { get; set; }
        public string roleID { get; set; }
        public bool IsCreate { get; set; }
        public bool IsRead { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
        public bool IsSubmit1 { get; set; }
        public bool IsSubmit2 { get; set; }
        public bool IsApprove { get; set; }
        public string Desc { get; set; }
        public string Status { get; set; }
        public virtual Permissions Permissions { get; set; }
    }
}
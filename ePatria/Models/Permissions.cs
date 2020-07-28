using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class Permissions
    {
        [Key]
        public int permissionID { get; set; }
        public string PermissionName { get; set; }
        public string Desc { get; set; }
        public string Status { get; set; }
    }
}
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class UserLoginDetail
    {
        [Key]
        public int UserLoginID { get; set; }
        public string UserID { get; set; }
        public DateTime? LastActivity { get; set; }
        public string status { get; set; }
    }
}
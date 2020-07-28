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
    public class BPM
    {
        [Key]
        public int BPMID { get; set; }
        public int WalktroughID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public virtual Walktrough Walktrough { get; set; }

    }
}
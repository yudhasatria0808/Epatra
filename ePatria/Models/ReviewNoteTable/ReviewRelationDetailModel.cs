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
    public class ReviewRelationDetail
    {

        public int ReviewRelationDetailID { get; set; }
        public int? ReviewRelationMasterID { get; set; }
        public int? PostId { get; set; }

        public virtual ReviewRelationMaster ReviewRelationMasters { get; set; }
        public virtual Post Posts { get; set; }
    }
}
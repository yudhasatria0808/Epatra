using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
//using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ePatria.Models
{
    public class ReviewRelationMaster
    {
        public ReviewRelationMaster()
        {
            this.ReviewRelationDetails = new HashSet<ReviewRelationDetail>();
        }

        public int ReviewRelationMasterID { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ReviewRelationDetail> ReviewRelationDetails { get; set; }
    }
}
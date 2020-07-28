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
    public class Post
    {
        public Post()
        {
            this.PostComments = new HashSet<PostComment>();
            this.ReviewRelationDetails = new HashSet<ReviewRelationDetail>();
        }

        public int PostId { get; set; }
        public string Message { get; set; }
        //public int PostedBy { get; set; }
        public string PostedBy { get; set; }
        public System.DateTime PostedDate { get; set; }

        public virtual ICollection<PostComment> PostComments { get; set; }
        public virtual ICollection<ReviewRelationDetail> ReviewRelationDetails { get; set; }
      
    }
}
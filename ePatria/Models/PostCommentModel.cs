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
    public class PostComment
    {
        public int PostCommentId { get; set; }
        public int PostId { get; set; }
        public string Message { get; set; }
        //public int CommentedBy { get; set; }
        public string CommentedBy { get; set; }
        public System.DateTime CommentedDate { get; set; }

        public virtual Post Post { get; set; }
    }
}
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
    public class ListFeedbackSended
    {
        [Key]
        public int Id { get; set; }
        public int FeddbackId { get; set; }
        public int PicId { get; set; }
    }
}
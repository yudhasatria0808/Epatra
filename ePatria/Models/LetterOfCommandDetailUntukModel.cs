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
    public class LetterOfCommandDetailUntuk
    {
        public int ID { get; set; }
        public int LetterOfCommandID { get; set; }
        public string Untuk { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }

    }
}
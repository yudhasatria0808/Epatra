using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class LetterOfCommandDetailTembusan
    {
        public int ID { get; set; }
        public int LetterOfCommandID { get; set; }
        public string Tembusan { get; set; }
        public virtual LetterOfCommand LetterOfCommand { get; set; }
    }
}
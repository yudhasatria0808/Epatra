﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingLetterOfCommandMember
    {
        [Key]
        public int ConsultingLetterOfCommandMemberID { get; set; }
        public int ConsultingSuratPerintahID { get; set; }
        public int MemberID { get; set; }
        public virtual ConsultingLetterOfCommand ConsultingLetterOfCommand { get; set; }
    }
}
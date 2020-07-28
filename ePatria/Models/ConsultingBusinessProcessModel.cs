using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePatria.Models
{
    public class ConsultingBusinessProcess
    {
        [Key]
        public int ConsultingBPMID { get; set; }
        public int? ConsultingWalktroughID { get; set; }
        public string DocumentNo { get; set; }
        public string DocumentName { get; set; }
        public string FolderName { get; set; }
        public virtual ConsultingWalktrough ConsultingWalktrough { get; set; }
        public virtual ICollection<ConsultingRiskControlMatrix> ConsultingRiskControlMatrixs { get; set; }
    }
}
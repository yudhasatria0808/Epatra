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
    public class ReportingBabModel
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Reporting")]
        public int IdReporting { get; set; }
        public int Bab { get; set; }
        public string JudulBab { get; set; }
        public string Description { get; set; }
        public string Fact { get; set; }
        public string Criteria { get; set; }
        public string Impact { get; set; }
        public string impactValue { get; set; }
        public string Cause { get; set; }
        public string Recommendation { get; set; }
        
        public virtual Reporting Reporting { get; set; }

        public ReportingBabModel()
        {

        }
        public ReportingBabModel(RCMDetailRiskControlIssue dbitem)
        {
            JudulBab = dbitem.Title;
            Fact = dbitem.Fact;
            Criteria = dbitem.Criteria;
            Impact = dbitem.Impact;
            impactValue = dbitem.ImpactValue;
            Cause = dbitem.Cause;
            Recommendation = dbitem.Recommendation;
        }
    }
}
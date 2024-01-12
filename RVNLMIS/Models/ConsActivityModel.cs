using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ConsActivityModel
    {

        public int EntActId { get; set; }

        public int ConsActID { get; set; }
        public int ActGId { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public string ActivityGroup { get; set; }
        public string ActUnit { get; set; }
        public string Unit { get; set; }
        public int? Selected { get; set; }
        public double? OriginalQty { get; set; }

        public int? SectionID { get; set; }

        public int? EntityID { get; set; }
        public bool IsDelete { get; set; }
    }

    public class drpSectionModel
    {
        public int SectionID { get; set; }
        public string SectionName { get; set; }
    }

    public class drpEntityModel
    {
        public int EntityID { get; set; }
        public string EntityName { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PackageStatusModel
    {
        public string EDName { get; set; }
        public string CPMCode { get; set; }
        public string PackageCode { get; set; }
        public string PackageShortName { get; set; }
        public string PackageName { get; set; }
        public Nullable<int> SectionCount { get; set; }
        public Nullable<int> EntityCount { get; set; }
        public Nullable<int> EnggDwgCount { get; set; }
        public Nullable<int> MaterialAssignedCount { get; set; }
        public Nullable<int> MaterialDataUpdatedCount { get; set; }
        public Nullable<int> ConActCount { get; set; }
        public Nullable<int> ConActDataUpdateCount { get; set; }
        public Nullable<int> InvoiceCount { get; set; }
        public Nullable<int> Points { get; set; }
    }
}
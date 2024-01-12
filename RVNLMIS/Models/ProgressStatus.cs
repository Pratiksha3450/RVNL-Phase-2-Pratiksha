using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ProgressStatus
    {
        public int SId { get; set; }
        public string SName { get; set; }
    }

    public class PackageActivityModel
    {
        public int ActivityId { get; set; }
        public string Activity { get; set; }
        public int Seqense { get; set; }
    }
}
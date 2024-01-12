using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIcountSummary
    {
        public int? AddCount { get; set; }

        public int? RejectedCount     { get; set; }

        public int? AssignedCount { get; set; }

        public int? ApprovedCount { get; set; }

        public int? ClosedCount { get; set; }

        public int? PartiallyAppCount { get; set; }

        public int? RevisedCount { get; set; }

        public int? ReOpenCount { get; set; }

        public int? ReScheduledCount { get; set; }

        public int? ScheduledCount { get; set; }
    }
}
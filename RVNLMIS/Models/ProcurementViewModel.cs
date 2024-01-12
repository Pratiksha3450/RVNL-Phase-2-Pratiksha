using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ProcurementViewModel
    {
        public int PackMatId { get; set; }

        public string MaterialName{ get; set; }

        public string MaterialUnit { get; set; }

        public string Discipline { get; set; }

        public string RatePerUnit{ get; set; }

        public decimal? OriginalQty{ get; set; }

        public double? CumOrderdQty { get; set; }

        public double? CumDeliveredQty { get; set; }

        public decimal? RevisedQty { get; set; }

        public int? PackageId{ get; set; }

        public int? DispId{ get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ConstructionViewModel
    {
        public int AutoId { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public string ActivityName { get; set; }

        public string ActivityUnit { get; set; }

        public decimal? BudgetedQty { get; set; }

        public double? CompleteQtyToDate { get; set; }

        public double? RevisedQty { get; set; }

        public int? PackageId { get; set; }

        public int ActivityId { get; set; }

        public int EntityId { get; set; }

        public string OperationType { get; set; }
    }
}

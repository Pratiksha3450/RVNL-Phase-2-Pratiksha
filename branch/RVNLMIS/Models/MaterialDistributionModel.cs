using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class MaterialDistributionModel
    {
        public int PackageId { get; set; }
        public int EMDId { get; set; }
        public int PackMatId { get; set; }
        public int EntityID { get; set; }
        public string EntityType { get; set; }
        public int MaterialId { get; set; }
        public string EntityCode { get; set; }
        public string EntityName  { get; set; }
        public string EntityQty  { get; set; }
        public double? EMOrderedQty { get; set; }
        public double? EMDeliveredQty { get; set; }
    }

    public class MaterialQtyModel
    {
        public double? TUsedOrderedQty { get; set; } = 0;
        public double? TUsedDeliveredQty { get; set; } = 0;
        public double? TRemainOrderedQty { get; set; } = 0;
        public double? TRemainDeliveredQty { get; set; } = 0;

        public double? TUsedOrderedQtyPerc { get; set; } = 0;
        public double? TUsedDeliveredQtyPerc { get; set; } = 0;
    }
}
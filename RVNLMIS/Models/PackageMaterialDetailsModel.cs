using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PackageMaterialDetailsModel
    {
        public int PkgMatId { get; set; }
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; }
        public string DisciplineName { get; set; }
        public string MaterialName { get; set; }
        public int DispID { get; set; }
        public string Unit { get; set; }
        public double? OriginalQty { get; set; }
        public decimal? RatePerUnit { get; set; }
    }
}

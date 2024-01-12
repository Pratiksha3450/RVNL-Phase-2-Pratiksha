using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class MaterialMasterModel
    {
        public int MaterialID { get; set; }
        public string MaterialCode { get; set; }
        public int DispID { get; set; }
        public string DispName { get; set; }
        public string MaterialName { get; set; }
        public string MaterialUnit { get; set; }
        public int? Selected { get; set; }
    }
    public class drpOptions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }
}
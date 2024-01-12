using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PackageList
    {
        public int PkgId { get; set; }
        public string PackageName { get; set; }
        public bool Isdeleted { get; set; }
    }
}
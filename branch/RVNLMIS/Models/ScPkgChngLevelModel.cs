using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ScPkgChngLevelModel
    {
        public int AutoId { get; set; }

        public int CrossID { get; set; }

        public string PackageName{ get; set; }

        public string CrossSectionName { get; set; }

        public string GridCloCS { get; set; }

        public string Chainage { get; set; }

        public string OGL { get; set; }

        public string FRL { get; set; }        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class BreadCrumbModel
    {
        public string MenuName { get; set; }

        public string MainParent { get; set; }

        public string SubParent { get; set; }
    }
}
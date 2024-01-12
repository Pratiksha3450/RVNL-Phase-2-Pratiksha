using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Models
{
    public class MenuModel
    {
        public int MenuID { get; set; }

        public int? MenuParentID { get; set; }

        [Required]
        public string MenuName { get; set; }

        [Required]
        public string Url { get; set; }

        public string ListId { get; set; }

        public string Icon { get; set; }

        public int? MenuOrder { get; set; }

        public int IsReportMenu { get; set; }

        [Required]
        public string MenuCode { get; set; }

        public string ParentMenuName { get; set; }

        public string Description { get; set; }

        public bool IsRFIMenu{ get; set; }
    }
}
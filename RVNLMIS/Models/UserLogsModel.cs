using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms.VisualStyles;

namespace RVNLMIS.Models
{
    public class UserLogsModel
    {
        public int UserId { get; set; }
        public int AuditID { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public string AreaAccessedU { get; set; }
        public string AccessedDate { get; set; }
        public string RoleName { get; set; }
        public string AccessedTime { get; set; }

        public string Icon { get; set; }

        public string FormName { get; set; }
        public string ActionDone { get; set; }
        public int PackageID { get; set; }

        public string MACAddress { get; set; }

        public string UpdatedValue { get; set; }
    }
}
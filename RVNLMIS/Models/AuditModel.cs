using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class AuditModel
    {
        // Audit Properties
        public int UserId { get; set; }
        public int AuditID { get; set; }
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public string AreaAccessed { get; set; }
        public DateTime? TimeAccessed { get; set; }

        public string GroupArea { get; set; }
    }
}
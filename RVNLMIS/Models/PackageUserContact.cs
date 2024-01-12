using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PackageUserContact
    {
        public int AutoId { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string WhatsappNo { get; set; }
        public bool? IsAppUser { get; set; }
        public int? NoOfUser { get; set; }
    }
}
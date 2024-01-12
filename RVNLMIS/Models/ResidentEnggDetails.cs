using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ResidentEnggDetails
    {
        public int WId { get; set; }
        public int? AutoId { get; set; }
        public string ResidentEngineerName { get; set; }
        public string ResidentEngineerName2 { get; set; }
        public string ReWhatsAppNum { get; set; }
        public string Re2WhatsAppNum { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
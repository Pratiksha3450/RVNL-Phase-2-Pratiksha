using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.Config
{
    public class CreateInvoiceDuePeriod
    {
        public int InvDID { get; set; }
        public int ProjId { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? ApprovalPeriod { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? PaymentPeriod { get; set; }
    }
}
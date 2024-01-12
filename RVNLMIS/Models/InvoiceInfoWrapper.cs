using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class InvoiceInfoWrapper
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<InvoicePaymentDetail> InvoiceDetails { get; set; }

    }
}
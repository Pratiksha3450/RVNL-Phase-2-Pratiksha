using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class InvoicePayment
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        [Required(ErrorMessage = "Required")]
        public string PaymentDates { get; set; }
        public int PaymentId { get; set; }

        public decimal? PaidAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string AttachFilePath { get; set; }
        public string AttachFileName { get; set; }
        public int AttachmentId { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }
        public bool? IsDeleted { get; set; }

        [Required(ErrorMessage = "Required")]
        public string PaidAmountString { get; set; }
        public bool IsPaid { get; set; }
        public string Remark { get; set; }
    }
}
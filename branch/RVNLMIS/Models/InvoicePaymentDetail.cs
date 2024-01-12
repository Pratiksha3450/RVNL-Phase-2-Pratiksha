using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class InvoicePaymentDetail
    {
        public int InvoiceId { get; set; }
        public int ProjectId { get; set; }
        public int PackageId { get; set; }
        public int AttachmentId { get; set; }
        public string PackageName { get; set; }
        public string ProjectName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string InvoiceNo { get; set; }
        [Required(ErrorMessage = "Required")]
        public string InvoiceDates { get; set; }
        public DateTime? InvoiceDate { get; set; }
        
        public decimal? InvoiceAmount { get; set; }
        public decimal? CertifiedAmount { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }

        [Required(ErrorMessage = "Required")]
        public string InvoiceAmountString { get; set; }
        [Required(ErrorMessage = "Required")]
        public string CertifiedAmountString { get; set; }
    }
}
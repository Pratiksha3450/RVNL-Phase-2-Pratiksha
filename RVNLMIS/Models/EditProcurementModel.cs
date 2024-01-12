using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class EditProcurementModel
    {
        public int PckMatId { get; set; }

        public string PackageName { get; set; }

        public string MaterialName { get; set; }

        public string RatePerUnit { get; set; }

        public string OriginalQty { get; set; }

        public string PackageCode { get; set; }

        public string RevisedQty { get; set; }

        public string OrderedQty { get; set; }

        public string DeliveredQty { get; set; }

        //public ExistingProcList OProc { get; set; }
    }

    public class ExistingProcList
    {
        public int PackMatTransId { get; set; }

        public int PackMatId { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialUnit { get; set; }
         
        public decimal RatePerUnit { get; set; }
 
        public double? OriginalQty { get; set; }

        public DateTime? TransDate { get; set; }

        [Required(ErrorMessage = "The Transaction Date is required.")]
        public string StrTransDate { get; set; }
       
        public double? RevisedQty { get; set; }

        public double? OrderedQty { get; set; }

        public double? DeliveredQty { get; set; }

        public string SupplierName { get; set; }

        public string AttachFilePath { get; set; }

        public string AttachFileName { get; set; }

        //[RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.pdf)$", ErrorMessage = "Only .jpg, .png, .pdf files allowed.")]
        public HttpPostedFileBase AttachmentFile { get; set; }

        public string PORef { get; set; }

        public DateTime? TargetDate { get; set; }

        public string StrTargetDate { get; set; }

        public string Remark { get; set; }

        public string OperationType { get; set; }
    }
}
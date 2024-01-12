using RVNLMIS.Common.ActionFilters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ConstActivityViewModel
    {
        public int ConsTranId { get; set; }

        public int EntityActId { get; set; }

        public int entityActvity { get; set; }

        public string ActivityName { get; set; }

        public string PackageName { get; set; }

        public string EntityName { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string ActivityUnit { get; set; }

        [Required(ErrorMessage = "The Transaction Date is required.")]
        public string StrTransDate { get; set; }

        public decimal? RevisedQty { get; set; }

        public decimal? BudgetedQty { get; set; }

        public decimal? CompletedQty { get; set; }

        public DateTime? TargetDate { get; set; }

        public string StrTargetDate { get; set; }

        public string Remark { get; set; }

        public string OperationType { get; set; }

        public string AttachFilePath { get; set; }

        public string AttachFileName { get; set; }

        //[ValidateFile]
        public HttpPostedFileBase AttachmentFile { get; set; }

        public string PackageCode { get; set; }
    }
}
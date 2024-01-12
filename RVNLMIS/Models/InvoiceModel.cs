using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class InvoiceModel
    {
        public int InvoiceId { get; set; }

        public Nullable<int> ProjectId { get; set; }

        public Nullable<int> PackageId { get; set; }

        public string InvoiceNo { get; set; }

        public string PackageName { get; set; }

        public string PackageCode { get; set; }

        public string ProjectName { get; set; }

        public string InvoiceDate { get; set; }

        public Nullable<decimal> InvoiceAmount { get; set; }

        public Nullable<decimal> CertifiedAmount { get; set; }

        public string Remark { get; set; }

        public Nullable<bool> IsPaid { get; set; }

        public string CreatedOn { get; set; }

        public Nullable<bool> IsDeleted { get; set; }



        public virtual tblMasterProject tblMasterProject { get; set; }

        public virtual tblPackage tblPackage { get; set; }

    }
}
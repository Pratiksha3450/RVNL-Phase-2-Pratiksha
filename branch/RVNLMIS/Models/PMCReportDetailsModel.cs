using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PMCReportDetailsModel
    {
        public int PMCReportId { get; set; }
        public int? PRId { get; set; }
        public int PackageId { get; set; }
        public int? AttachmentID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Remark { get; set; }
        public string AttachFilePath { get; set; }
        public string AttachFileName { get; set; }
        //[ValidateFile]
        [Required]
        public HttpPostedFileBase AttachmentFile { get; set; }
        public DateTime? ReportingDate { get; set; }
        [Required]
        public string ReportingDates { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string PMCReportType { get; set; }
        public string PackageName { get; set; }

    }
}
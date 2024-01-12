using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PowerbiReportData
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ReportName { get; set; }

        public string ReportImage { get; set; }

        [Required(ErrorMessage = "Required")]
        public string WorkSpaceId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ApplicationId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ApplicationSecret { get; set; }

        [Required(ErrorMessage = "Required")]
        public string TenantId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ReportId { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? MenuId { get; set; }
    

        public string MenuName { get; set; }

        public int? GroupId { get; set; }
        public string GroupName { get; set; }

        public string DatasetId { get; set; }

        public bool isDashboard { get; set; }

        public string URL { get; set; }

        public Nullable<bool> isDeleted { get; set; }

        public HttpPostedFileBase Image { get; set; }
    }
}
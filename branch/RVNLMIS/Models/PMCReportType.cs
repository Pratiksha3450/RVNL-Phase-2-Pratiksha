using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PMCReportType
    {
        public int PRId { get; set; }
        [Required(ErrorMessage = "required")]
        public string PMCReportTypeName { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
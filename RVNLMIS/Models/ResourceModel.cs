using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class ResourceModel
    {
        public int ResourceId { get; set; }

        [Required(ErrorMessage = "required")]
        public int? PackageId { get; set; }

        public string PackageName { get; set; }

        [Required(ErrorMessage = "required")]
        public string ResourceName { get; set; }

        [Required(ErrorMessage = "required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = " must be numeric")]
        public string ResourceUnit { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
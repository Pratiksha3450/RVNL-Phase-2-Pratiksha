using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class SectionModel
    {
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Required")]

        public int PackageId { get; set; }
        public string PackageName { get; set; }

        public string PackageCode { get; set; }

        [Required(ErrorMessage = "Required")]
        public string SectionName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string SectionCode { get; set; }
        public DateTime? SectionStart { get; set; }
        public string SectionStarts { get; set; }
        public DateTime? SectionFinish { get; set; }
        public string SectionFinishs { get; set; }
        //[RegularExpression("^[0-9]*$", ErrorMessage = " must be numeric")]
        public string StartChaining { get; set; }
        //[RegularExpression("^[0-9]*$", ErrorMessage = " must be numeric")]
        public string EndChaining { get; set; }
        public double? Length { get; set; }
        public string SectionLength { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
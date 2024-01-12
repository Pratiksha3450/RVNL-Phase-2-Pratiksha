using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class TargetSectionModel
    {
        public int TargetSectionId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int PackageId { get; set; }


        [Required(ErrorMessage = "Required")]
        public int SectionId { get; set; }


        [Required(ErrorMessage = "Required")]
        public int? Year { get; set; }


        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public int AddedBy { get; set; }


        [NotMapped]
        public string PackageName { get; set; }
        [NotMapped]
        public string SectionName { get; set; }

        [NotMapped]
        public string YearStr { get; set; }

    }
}
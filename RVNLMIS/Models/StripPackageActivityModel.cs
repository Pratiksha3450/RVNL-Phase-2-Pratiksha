using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class StripPackageActivityModel
    {
        public int StripActId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int DispId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int ConsActId { get; set; }
        public string ActivityName { get; set; }
        public string PackageName { get; set; }
        public string DisciplineName { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Sequence { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Color { get; set; }
    }
}
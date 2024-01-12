using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class StripProgressModel
    {
        public int ActProgressId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int StripActId { get; set; }
        public int Sequence { get; set; }

        [Required(ErrorMessage = "Required")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Required")]        
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DataDate { get; set; }
        
        public int ConsActId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string StartChainage { get; set; }
        [Required(ErrorMessage = "Required")]
        public string EndChainage { get; set; }
        public string Lenght { get; set; }
        [Required(ErrorMessage = "Required")]
        public int Sid { get; set; }
        public string Status { get; set; }
        public string PackageName { get; set; }
        public string ActivityName { get; set; }


        [Required(ErrorMessage = "Required")]
        public int DispId { get; set; }
        public string DisciplineName { get; set; }

        public double StartC { get; set; }
        public double EndC { get; set; }

    }
}
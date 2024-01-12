using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class UpdateProgressChartModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Required")]
        public int PackageId { get; set; }
        [Required(ErrorMessage = "Required")]
        //[DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DataDate { get; set; }
        [Required(ErrorMessage = "Required")]
        public int ActivityId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string StartChainage { get; set; }
        [Required(ErrorMessage = "Required")]
        public string EndChainage { get; set; }
        public string Lenght { get; set; }
        [Required(ErrorMessage = "Required")]
        public int Sid { get; set; }
        public string Status { get; set; }
        public string PackageName { get; set; }
        public string Activity { get; set; }
        public string PackageCode { get;  set; }
        public string PackageShortName { get;  set; }
    }
}
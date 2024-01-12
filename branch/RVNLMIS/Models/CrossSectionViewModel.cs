using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class CrossSectionViewModel
    {

    }

    public class ScActivityDetailsModel
    {
        public int ScActID { get; set; }

        public int? PackageId { get; set; }

        [Required(ErrorMessage ="Activity Name is required.")]
        public string ScActName { get; set; }

        //[Required(ErrorMessage = "Please ch.")]
        public string PlotColour { get; set; }

        [Required(ErrorMessage = "required.")]
        public double PlotThk { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class ScCrossSectionDetailsModel
    {
        public int AutoID { get; set; }

        [Required(ErrorMessage ="*")]
        public int? CrossID { get; set; }

        public int? CsDPackageId { get; set; }

        [Required(ErrorMessage ="*")]
        public int? ScActivityID { get; set; }

        public string ActivityName { get; set; }

        public string CrossSectionName { get; set; }

        public bool Layer { get; set; }

        public string StrLayer { get; set; }

        public int? SeqNo { get; set; }

        [Required(ErrorMessage = "*")]
        public double? TotalThk { get; set; }

        [Required(ErrorMessage = "*")]
        public double? MaxLayerThk { get; set; }

        [Required(ErrorMessage = "*")]
        public double? Slope { get; set; }

        [Required(ErrorMessage = "*")]
        public double? TopWd { get; set; }

        [Required(ErrorMessage = "*")]
        public double? BottomWd { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }


    public class ScPkgCrossSectionModel
    {
        public int CsID { get; set; }

        public int? CSPackageId { get; set; }

        [Required(ErrorMessage ="Section name is required.")]
        public string CSName { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
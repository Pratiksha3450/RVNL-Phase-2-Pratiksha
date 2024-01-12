using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIBOQMasterModel
    {
        public int BoqID { get; set; }

        [Required(ErrorMessage ="required")]
        public string BoqCode { get; set; }

        [Required(ErrorMessage = "required")]
        public string BoqName { get; set; }

        [Required(ErrorMessage = "required")]
        public string BoqUnit { get; set; }

        [Required(ErrorMessage = "required")]
        public double? BoqRate { get; set; }

        [Required(ErrorMessage = "required")]
        public double? BoqQty { get; set; }
    }
}
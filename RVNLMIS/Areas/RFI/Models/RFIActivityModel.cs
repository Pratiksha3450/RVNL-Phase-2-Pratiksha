using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIActivityModel
    {
        public int RFIActId { get; set; }

        [Required(ErrorMessage = "required")]
        public int ConsActId { get; set; }

        [Required(ErrorMessage = "required")]
        public int WorkGroupId { get; set; }

        [Required(ErrorMessage = "required")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "required")]
        public string RFIActName { get; set; }


        public string ActityName { get; set; }
        public string WorkGrName { get; set; }
    }
}
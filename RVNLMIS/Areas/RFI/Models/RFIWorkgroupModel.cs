using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIWorkgroupModel
    {
        public int WorkgroupId { get; set; }

        [Required(ErrorMessage ="required")]
        public string WorkgroupName { get; set; }


        [Required(ErrorMessage = "required")]
        public int? DispId { get; set; }

        public string Discipline { get; set; }

        [Required(ErrorMessage = "required")]
        public string WorkgroupPrefix { get; set; }
    }
}
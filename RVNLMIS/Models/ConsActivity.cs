using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ConsActivity
    {
        public int ConsActId { get; set; }
        public int ActGId { get; set; }

        public string ActivityGroup { get; set; }
        [Required(ErrorMessage = "required")]
        public string ActivityCode { get; set; }
        [Required(ErrorMessage = "required")]
        public string ActivityName { get; set; }
        public string ActUnit { get; set; }
        public float? ActWtg { get; set; }
        public bool? IsDelete { get; set; }
        public bool isStripChart { get; set; }
        public bool IsRFI { get; set; }

        [NotMapped]
        public string StripChart { get; set; }

        [NotMapped]
        public string RFI { get; set; }


    }
}
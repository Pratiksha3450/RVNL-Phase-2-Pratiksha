using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace RVNLMIS.Areas.RFI.Models
{
    public class InspStatusModel
    {
        public int InspId { get; set; }        

        [Required(ErrorMessage = "required")]

        public string InspDesc { get; set; }

        [Required(ErrorMessage = "required")]
        public string StatusType { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }   // Added
    }
}
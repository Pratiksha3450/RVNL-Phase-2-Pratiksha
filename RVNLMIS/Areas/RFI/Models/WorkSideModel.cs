using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace RVNLMIS.Areas.RFI.Models
{
    public class WorkSideModel
    {
        public int WorkSideId { get; set; }        

        [Required(ErrorMessage = "required")]
        public string WorkSideName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }   // Added
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace RVNLMIS.Areas.RFI.Models
{
    public class EnclouserModel
    {
        public int EnclId { get; set; }        

        [Required(ErrorMessage = "required")]
        public string EnclName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }   // Added
    }
}
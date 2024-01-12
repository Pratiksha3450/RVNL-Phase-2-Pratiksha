using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class EDMasterModel
    {
        public int EDId { get; set; }
       
        public string EDCode { get; set; }

        [Required(ErrorMessage = "ED Name is required")]
        public string EDName { get; set; }
    }
}
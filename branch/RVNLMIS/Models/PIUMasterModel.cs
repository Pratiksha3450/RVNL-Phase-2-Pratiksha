using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PIUMasterModel
    {
        public int PIUId { get; set; }

        public int EDId { get; set; }

        [Required(ErrorMessage = "PIU code is required")]
        public string PIUCode { get; set; }

        [Required(ErrorMessage = "PIU Name is required")]
        public string PIUName { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        public decimal Logitude { get; set; }

        public string EDName { get; set; }
    }
}
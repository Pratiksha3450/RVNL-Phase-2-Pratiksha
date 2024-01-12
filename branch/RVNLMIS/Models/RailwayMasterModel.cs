using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class RailwayMasterModel
    {
        public int RailwayId { get; set; }

        [Required(ErrorMessage = "Railway code is required")]
        public string RailwayCode { get; set; }

        [Required(ErrorMessage = "Railway Name is required")]
        public string RailwayName { get; set; }
    }
}
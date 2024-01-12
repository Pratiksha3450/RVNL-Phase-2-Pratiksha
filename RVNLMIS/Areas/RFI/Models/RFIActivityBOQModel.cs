using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIActivityBOQModel
    {
        public int? RFIActBOQId { get; set; }




        [Required(ErrorMessage = "required")]
        public int RFIActId { get; set; }


        [Required(ErrorMessage = "required")]
        public int RFIBOQId { get; set; }



        [Required(ErrorMessage = "required")]
        public string RFIBOQCode { get; set; }

        [NotMapped]
        public string RFIActName { get; set; }
        [NotMapped]
        public string RFIBOQName { get; set; }

        public bool IsDelete { get; set; }


    }
}
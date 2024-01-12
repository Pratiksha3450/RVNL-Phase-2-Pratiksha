using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.Config
{
    public class CreatePhyProgress
    {
        public int PhyID { get; set; }
        public int? ProjId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string PhysicalParamCode { get; set; }

        [Required(ErrorMessage = "Required")]
        public string PhysicalParamName { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.Config
{
    public class CreateHseParam
    {

        public int HPID { get; set; }
        public int ProjId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? HseParamSeq { get; set; }

        [Required(ErrorMessage = "Required")]
        public string HseParamName { get; set; }
    }
}
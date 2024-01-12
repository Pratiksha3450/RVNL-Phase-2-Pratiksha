using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.Config
{
    public class CreateQaQcParam
    {
        public int QPID { get; set; }
        public int ProjId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? QParSeq { get; set; }

        [Required(ErrorMessage = "Required")]
        public string QParName { get; set; }
    }
}
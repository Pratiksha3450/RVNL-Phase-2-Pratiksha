using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.Config
{
    public class CreateKPIParam
    {
        public int KPID { get; set; }
        public int ProjId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? KpiSequence { get; set; }

        [Required(ErrorMessage = "Required")]
        public string KpiParameter { get; set; }

        public string Remark { get; set; }
    }
}
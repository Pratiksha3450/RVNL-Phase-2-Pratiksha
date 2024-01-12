using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.Config
{
    public class CreateCashParam
    {
        public int CashID { get; set; }
        public int ProjId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string CashParameter { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ProjectCurrency { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class EnggApprGateModel
    {
        public int ApprGateId { get; set; }

        [Required(ErrorMessage ="required")]
        public string AppGateName { get; set; }

        [Required(ErrorMessage ="required")]
        //[MaxLength(12)]
        //[MinLength(1)]
        [RegularExpression("^[0-9]*$", ErrorMessage = " must be numeric")]
        //[Required(ErrorMessage ="required")]
        public int Sequence { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class EnggDwgTypeModel
    {
        public int DwgId { get; set; }

        [Required(ErrorMessage = "required")]
        public string DwgName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
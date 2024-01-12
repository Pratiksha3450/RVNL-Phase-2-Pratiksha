using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ActivityGroup
    {
        public int ActGId { get; set; }
        [Required(ErrorMessage = "required")]
        public string ActivityGroupName { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
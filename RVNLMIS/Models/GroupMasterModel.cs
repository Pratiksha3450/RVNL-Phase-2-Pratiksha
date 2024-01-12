using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class GroupMasterModel
    {
        public int GroupId { get; set; }
        public string GroupCode { get; set; }
        [Required(ErrorMessage = "Group Name is required")]
        public string GroupName { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class RoleMenuAccessModel
    {
        public int RoleMenuID { get; set; }
        [Required]
        public int RoleID { get; set; }

        public string role { get; set; }
        public string MenuName { get; set; }
    }
}
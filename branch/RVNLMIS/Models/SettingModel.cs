using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class SettingModel
    {
        public int SettingID { get; set; }
        [Required(ErrorMessage = "required")]
        public string SKey { get; set; }
        [Required(ErrorMessage = "required")]
        public string Value { get; set; }
        [Required(ErrorMessage = "required")]
        public string DataType { get; set; }
        public DateTime? CreateOn { get; set; }
        public bool? IsDelete { get; set; }
    }
}
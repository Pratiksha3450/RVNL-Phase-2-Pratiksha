using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ProcMaterial
    {

        public int MaterialId { get; set; }
        [Required(ErrorMessage = "required")]
        public int DispId { get; set; }
        public string DispName { get; set; }
        [Required(ErrorMessage = "required")]
        public string MaterialCode { get; set; }
        [Required(ErrorMessage = "required")]
        public string MaterialName { get; set; }
        public string MaterialUnit { get; set; }
        public bool? IsDelete { get; set; }
    }
}
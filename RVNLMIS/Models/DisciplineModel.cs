using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace RVNLMIS.Models
{
    public class DisciplineModel
    {
        public int DisciplineId { get; set; }

        [Required(ErrorMessage = "required")]
        public string DisciplineCode { get; set; }

        [Required(ErrorMessage = "required")]
        public string DisciplineName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }   // Added
    }
}
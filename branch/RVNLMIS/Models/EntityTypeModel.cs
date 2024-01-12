using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class EntityTypeModel
    {
        public int EntityTypeId { get; set; }
        public int? ActGId { get; set; }

        [Required(ErrorMessage = "required")]
        public string EntityTypeName { get; set; }
        public string ActivityGroupName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
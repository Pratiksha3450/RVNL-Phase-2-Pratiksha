using System;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class DisciplineUserModel
    {

        public int UserId { get; set; }

        [Required(ErrorMessage = "Required")]

        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]

        public string UserName { get; set; }

        public string Password { get; set; }

        [Required(ErrorMessage = "Required")]
        public int DisciplineId { get; set; }

        public string DisciplineName { get; set; }

        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]

        public string EmailId { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not valid")]
        public string MobileNo { get; set; }
        [Required(ErrorMessage = "Required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int RoleTableId { get; set; }
        public string RoleTableName { get; set; }
        public string RoleName { get; set; }
        public string TableDataName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }

    }
}
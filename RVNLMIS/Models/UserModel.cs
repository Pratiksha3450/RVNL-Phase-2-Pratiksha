using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Required")]

        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]

        public string UserName { get; set; }

        public string ProfilePic { get; set; }

        [Required(ErrorMessage = "Required")]

        public string Password { get; set; }

        public int Discipline { get; set; }

        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]

        public string EmailId { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not valid")]
        public string MobileNo { get; set; }


        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public string DesignationName { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string TableDataCode { get; set; }
        public int RoleTableID { get; set; }
        public string TableDataName { get; set; }
        public string RoleCode { get; set; }
    }

    
    public static class Enum_Role
    {
        //  Usage: string CPM = Enum_Role.CPM;
        public const string
            SAD = "SAD",
            SAU = "SAU",
            CPM = "CPM",
            EDM = "EDM",
            PRJ = "PRJ",
            PKG = "PKG";
    }

}
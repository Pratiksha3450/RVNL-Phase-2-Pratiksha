using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIUserModel
    {
        public int RFIUserId { get; set; }

        [Required (ErrorMessage ="required")]
        public int PackgeId { get; set; }

        [Required(ErrorMessage = "required")]
        public string FullName { get; set; }

        public string PackageName { get; set; }

        [Required(ErrorMessage = "required")]
        [EmailAddress (ErrorMessage ="Not Valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "required")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "required")]
        public string Organisation { get; set; }

        [Required(ErrorMessage = "required")]
        public int? DesignationId { get; set; }

        public string Designation { get; set; }

        [Required(ErrorMessage = "required")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[\\W_]).{6,}$",
         ErrorMessage = "Password should be minimum of 6 characters, with at least one uppercase, one lower case, one digit and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "required")]
        public int? ReportingTo { get; set; }

        public string ReportingToUser { get; set; }

        public bool SendNotification { get; set; }

        public string DeviceId { get; set; }
    }
}
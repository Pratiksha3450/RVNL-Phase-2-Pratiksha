using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ContactDetailsModel
    {
        public int AutoId { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public string PackageInfo { get; set; }
        [Required(ErrorMessage = "Required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter valid Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        //[Required(ErrorMessage = "Required")]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not valid")]
        //public string Mobile { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not valid")]
        public string WhatsappNo { get; set; }
        public bool IsAppUser { get; set; }
        [Required(ErrorMessage = "Required")]
        public int NoOfUser { get; set; }

        public int WId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ResidentEngineerName { get; set; }
        public string ResidentEngineerName2 { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not valid")]
        public string ReWhatsAppNum { get; set; }
        public string Re2WhatsAppNum { get; set; }
      
    }

}
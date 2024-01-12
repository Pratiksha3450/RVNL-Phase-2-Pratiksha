using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class LoginModel
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        public string returnUrl { get; set; }
        public string cookie { get; set; }
        public HttpCookie nameFormAuth { get; set; }

        public bool RememberMe { get; set; }

        //public int r1 { get; set; }
        //public int r2 { get; set; }
        //public int r3 { get; set; }

        public HttpCookie rloginCookie { get; set; }
        public string rloginCookie1 { get; set; }

        public bool isActiveIssue { get; set; }
    }
}
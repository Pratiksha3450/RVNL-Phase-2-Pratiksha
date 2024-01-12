using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ResponseModelView
    {
        public string Type { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public ResponseData Data { get; set; }
    }

    public class ResponseData
    {
        public string userid { get; set; }

        public string name { get; set; }

        public string username { get; set; }

        public string RoleId { get; set; }

        public int? Discipline { get; set; }

        public int? PackageId { get; set; }

        public string RoleCode { get; set; }

        public string Orgnisation { get; set; }

        public string Designation { get; set; }

        public int? DesignationId { get; set; }

        public string TableDataName{ get; set; }

        public string ContactNo { get; set; }

        public string EmailId{ get; set; }

        public bool AnyActiveIssue { get; set; }

        public string Xsignature { get; set; }
        public string UserToken { get; set; }
    }
}
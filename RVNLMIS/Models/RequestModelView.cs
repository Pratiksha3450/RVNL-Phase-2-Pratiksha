using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class RequestModelView
    {
        public string Type { get; set; }
        public RequestData Data { get; set; }
    }

    public class RequestData
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
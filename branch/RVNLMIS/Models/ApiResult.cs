using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ApiResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
    }
}
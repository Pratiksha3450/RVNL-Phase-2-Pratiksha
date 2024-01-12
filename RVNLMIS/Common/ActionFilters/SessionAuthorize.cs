
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Common.ActionFilters
{
    public class SessionAuthorize : AuthorizeAttribute
    {
        //Core authentication, called before each action
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
           // var url = httpContext.Request.Url;
            return (string.IsNullOrEmpty(Convert.ToString(httpContext.Session["UserData"]))) ? false : true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace RVNLMIS.Common.ActionFilters
{
    public class AreaSessionExpire : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {


            if (HttpContext.Current.Session["RFIUserSession"] == null)
            {
                FormsAuthentication.SignOut();
                filterContext.Result =
               new RedirectToRouteResult(new RouteValueDictionary
                 {
                    { "action", "Index" },
                    { "controller", "RFILogin" },
                    { "returnUrl", filterContext.HttpContext.Request.RawUrl}
                  });
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.DAC;
using RVNLMIS.Models;

namespace RVNLMIS.Common.ActionFilters
{
    public class AuditAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Stores the Request in an Accessible object
            var request = filterContext.HttpContext.Request;
            // Generate an audit
            tblAudit audit = new tblAudit()
            {
                // Your Audit Identifier     
               // AuditID = Guid.NewGuid(),
                // Our Username (if available)
                UserId= (request.IsAuthenticated) ? Convert.ToInt32(filterContext.HttpContext.User.Identity.Name.Split('|')[1]) : 0,
                UserName = (request.IsAuthenticated) ? filterContext.HttpContext.User.Identity.Name.Split('|')[0] : "Anonymous",
                // The IP Address of the Request
                IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                // The URL that was accessed
                AreaAccessed = request.RawUrl,
                // Creates our Timestamp
                TimeAccessed = DateTime.Now
            };

            // Stores the Audit in the Database
            dbRVNLMISEntities context = new dbRVNLMISEntities();
            context.tblAudits.Add(audit);
            context.SaveChanges();

            // Finishes executing the Action as normal 
            base.OnActionExecuting(filterContext);
        }
    }
}
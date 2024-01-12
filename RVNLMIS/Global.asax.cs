using RVNLMIS.Controllers;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RVNLMIS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Do whatever you want to do with the error

        ////Show the custom error page...
        //    Server.ClearError();
        //    var routeData = new RouteData();
        //    routeData.Values["controller"] = "Error";

        //    if ((Context.Server.GetLastError() is HttpException) && ((Context.Server.GetLastError() as HttpException).GetHttpCode() != 404))
        //    {
        //        routeData.Values["action"] = "Error";
        //    }
        //    else
        //    {
        //        // Handle 404 error and response code
        //        Response.StatusCode = 404;
        //        routeData.Values["action"] = "NotFound";
        //    }
        //    Response.TrySkipIisCustomErrors = true; // If you are using IIS7, have this line
        //    IController errorsController = new ErrorController();
        //    HttpContextWrapper wrapper = new HttpContextWrapper(Context);
        //    var rc = new System.Web.Routing.RequestContext(wrapper, routeData);
        //    errorsController.Execute(rc);

        //    Response.End();
        }

        void Application_BeginRequest()
        {
            if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
            {
                Response.Flush();
            }
        }
    }
}

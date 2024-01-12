using System.Web.Http;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI
{
    public class RFIAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "RFI";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapHttpRoute("RFI_WebApiRoute",
                 "RFI/API/{controller}/{id}",
                new { id = RouteParameter.Optional });

            context.MapRoute(
                "RFI_default",
                "RFI/{controller}/{action}/{id}",
                 defaults: new { controller = "RFILogin", action = "Index", id = UrlParameter.Optional }
            // new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
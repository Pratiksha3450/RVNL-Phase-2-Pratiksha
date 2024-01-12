using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RVNLMIS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );

   //         routes.MapRoute(
   //    name: "404-NotFound",
   //    url: "NotFound",
   //    defaults: new { controller = "Error", action = "NotFound" }
   //);

   //         routes.MapRoute(
   //             name: "500-Error",
   //             url: "Error",
   //             defaults: new { controller = "Error", action = "Error" }
   //         );

   //         //..other routes

   //         //I also put a catch all mapping as last route

   //         //Catch All InValid (NotFound) Routes
   //         routes.MapRoute(
   //             name: "NotFound",
   //             url: "{*url}",
   //             defaults: new { controller = "Error", action = "NotFound" }
   //         );
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace labQueue
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
            "AboutRoute",                                              // Route name
            "{controller}/{action}/{subaction}/{subaction_param}",                           // URL with parameters
            new { controller = "Home", action = "About", subaction = "", subaction_param = "" }  // Parameter defaults
            );

            routes.MapRoute(
            "ApiRoute",
            "{controller}/{action}/{signal_sender}/{signal_name}/{signal_param}",                           
            new { controller = "Laboratory", action = "Api", signal_sender = "", signal_name = "", signal_param="" }  
            );
        }
    }
}

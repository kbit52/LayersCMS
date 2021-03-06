﻿using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using AttributeRouting.Web.Mvc;
using LayersCMS.MvcApp.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(LayersCmsRouting), "Start")]

namespace LayersCMS.MvcApp.App_Start
{
    public static class LayersCmsRouting
    {
        public static void RegisterAttributeRoutes(RouteCollection routes)
        {
            // See http://github.com/mccalltd/AttributeRouting/wiki for more options.
            // To debug routes locally using the built in ASP.NET development server, go to /routes.axd

            routes.MapAttributeRoutes(config =>
            {
                config.AddRoutesFromAssembly(Assembly.GetExecutingAssembly());
                config.UseLowercaseRoutes = true;
            });
        }

        public static void RegisterCmsRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "LayersCmsHomePage", // Route name
                "", // URL with parameters
                new { controller = "LayersCmsPage", action = "HandleUrl", url = "/" } // Parameter defaults
            );

            routes.MapRoute(
                "LayersCmsPage", // Route name
                "{url}", // URL with parameters
                new { controller = "LayersCmsPage", action = "HandleUrl" } // Parameter defaults
            );

            // Default route is still required for child actions to function
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        public static void Start()
        {
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            RegisterAttributeRoutes(RouteTable.Routes);

            RegisterCmsRoutes(RouteTable.Routes);
        }
    }
}

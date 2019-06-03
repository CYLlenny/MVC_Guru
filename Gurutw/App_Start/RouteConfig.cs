using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gurutw
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //鍵盤分類頁
            routes.MapRoute(
               name: "Keyboard",
               url: "Keyboard",
               defaults: new { controller = "Home", action = "Keyboard_Category" }
           );

            //滑鼠分類頁
            routes.MapRoute(
               name: "Mouse",
               url: "Mouse",
               defaults: new { controller = "Home", action = "Mouse_Category" }
           );

            //耳機分類頁
            routes.MapRoute(
               name: "Headset",
               url: "Headset",
               defaults: new { controller = "Home", action = "Headset_Category" }
           );




            //鍵盤產品頁
            routes.MapRoute(
               name: "Keyboard_Product",
               url: "Keyboard/{id}",
               defaults: new { controller = "Home", action = "Keyboard_item", id = UrlParameter.Optional }
           );

            //滑鼠產品頁
            routes.MapRoute(
               name: "Mouse_Product",
               url: "Mouse/{id}",
               defaults: new { controller = "Home", action = "Mouse_item", id = UrlParameter.Optional }
           );

            //耳機產品頁
            routes.MapRoute(
               name: "Headset_Product",
               url: "Headset/{id}",
               defaults: new { controller = "Home", action = "Headset_item", id = UrlParameter.Optional }
           );



            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

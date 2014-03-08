using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Batch;
using WebApiContrib.Formatting;

namespace Damienbod.WebAPI.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //config.Routes.MapHttpBatchRoute(
            //    routeName: "WebApiBatch",
            //    routeTemplate: "api/$batch",
            //    batchHandler: new DefaultHttpBatchHandler(GlobalConfiguration.DefaultServer));

            config.Routes.MapHttpBatchRoute(
                routeName: "WebApiBatch",
                routeTemplate: "api/$batch",
                batchHandler: new ProtobufBatchHandler(GlobalConfiguration.DefaultServer));

            

            config.MapHttpAttributeRoutes();
            config.Formatters.Add(new ProtoBufFormatter());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using BusinessObjects;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

namespace XTrade.MainServer
{
    public class Startup
    {
        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(xtradeConstants.TOKEN_LIFETIME_HOURS),
                Provider = new DBAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }



        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            ConfigureOAuth(app);
            // Adding to the pipeline with our own middleware
            app.Use(async (context, next) =>
            {
                // Add Header
                context.Response.Headers["XTrade"] = "XTrade Web Api Self Host";
                context.Response.Headers["Access-Control-Allow-Origin"] = "*"; 
                // Call next middleware
                await next.Invoke();
            });

            // Custom Middleare
            //app.Use(typeof(CustomMiddleware));

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute("DefaultApiWithId", "Api/{controller}/{id}", new { id = RouteParameter.Optional }, new { id = @"\d+" });
            config.Routes.MapHttpRoute("DefaultApiWithAction", "Api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("DefaultApiGet", "Api/{controller}", new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute("DefaultApiPost", "Api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });
            config.MapHttpAttributeRoutes();

            //config.EnableCors();
            // Web Api
            app.UseWebApi(config);

            var hubConf = new HubConfiguration();
            hubConf.EnableDetailedErrors = true;
            hubConf.EnableJSONP = true;
            //hubConf.Resolver = new AutofacDependencyResolver(Program.Container);
            app.MapSignalR(hubConf);

            // Turn tracing on programmatically
            //GlobalHost.TraceManager.Switch.Level = SourceLevels.Information;

            SetupAngular(app);
        }

        public void SetupAngular(IAppBuilder app)
        {
            var physicalFileSystem = new PhysicalFileSystem(xtradeConstants.ANGULAR_DIR);
            var options = new FileServerOptions
            {
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };
            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[]
            {
            "index.html"
            };

            app.UseFileServer(options);
            // File Server
            /*          
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true,
                DefaultFilesOptions = { DefaultFileNames = { "index.html" } },
                FileSystem = new PhysicalFileSystem("Assets"),
                StaticFileOptions = { ContentTypeProvider = new CustomContentTypeProvider() }
            };

            app.UseFileServer(options);
            */

        }

    }
}

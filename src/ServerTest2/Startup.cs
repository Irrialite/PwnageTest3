using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.WebSockets.Server;
using Microsoft.AspNetCore.WebSockets.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.RollingFile;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace ServerTest2
{
    public class Startup
    {
        public IConfigurationRoot Configuration
        {
            get;
            set;
        }

        public Startup(IHostingEnvironment env)
        {
            var globalbuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("globalconfig.json");
            var globalConfiguration = globalbuilder.Build();

            string stagingEnvironment = globalConfiguration["StagingEnvironment"];

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{stagingEnvironment}.json", optional: true);
            Configuration = builder.Build();

            Directory.CreateDirectory(env.ContentRootPath + "/log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is((Serilog.Events.LogEventLevel)int.Parse(Configuration["Logging:LogLevel"]))
                .Enrich.FromLogContext()
                .WriteTo.RollingFile($"{env.ContentRootPath}/log/{{Date}}-log.txt", fileSizeLimitBytes: 10 * 1024 * 1024)
                .WriteTo.LiterateConsole()
                .CreateLogger();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider provider, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            //app.UseWebSockets();
            app.UseMiddleware<WebSocketMiddleware>();
            app.UseMiddleware<WebSockets.WebSocketMiddlewareImpl>();
            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<WebSockets.WebSocketServerManager>();
            services.AddSingleton<TCP.TCPServerManager>();
        }
    }
}

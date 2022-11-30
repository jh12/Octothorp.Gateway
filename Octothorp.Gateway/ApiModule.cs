using System.Collections.Generic;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Octothorp.Gateway.Middleware;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

namespace Octothorp.Gateway
{
    public class ApiModule : Module
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;

        public ApiModule(IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterLogger(builder);
            RegisterMiddleware(builder);
        }

        private void RegisterLogger(ContainerBuilder containerBuilder)
        {
            LoggerConfiguration builder = new LoggerConfiguration();

            builder
                .ReadFrom
                .Configuration(_configuration);

            builder
                .Enrich.WithUserName()
                .Enrich.WithMachineName()
                .Enrich.WithAssemblyName()
                .Enrich.WithAssemblyVersion()
                .Enrich.FromLogContext();

            if (_hostEnvironment.IsProduction())
            {
                builder.WriteTo.Console(new RenderedCompactJsonFormatter());
            }
            else
            {
                builder.WriteTo.Console();
            }

            Logger logger = builder.CreateLogger();
            Log.Logger = logger;

            containerBuilder.Register(f => logger).As<ILogger>().SingleInstance();
        }

        private void RegisterMiddleware(ContainerBuilder builder)
        {
            builder.RegisterType<LoggingContextMiddleware>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Octothorp.Gateway.Middleware;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

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
                .Enrich.WithUserName()
                .Enrich.WithMachineName()
                .Enrich.WithAssemblyName()
                .Enrich.WithAssemblyVersion()
                .Enrich.FromLogContext();

            builder
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                //.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .Enrich.FromLogContext();

            if (_hostEnvironment.IsDevelopment())
                builder.WriteTo.Console();
            else
            {
                builder.WriteTo.Console();

                IConfigurationSection lokiSection = _configuration.GetSection("Loki");

                if(lokiSection != null)
                {
                    string url = lokiSection["Url"];

                    builder.WriteTo.LokiHttp(() => new LokiSinkConfiguration()
                    {
                        LokiUrl = url,
                        LogLabelProvider = new LokiLabelProvider()
                    });
                }
            }

            Logger logger = builder.CreateLogger();
            Log.Logger = logger;

            containerBuilder.Register(f => logger).As<ILogger>().SingleInstance();

            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            Serilog.Debugging.SelfLog.Enable(Console.Error);
        }

        private void RegisterMiddleware(ContainerBuilder builder)
        {
            builder.RegisterType<LoggingContextMiddleware>();
        }
    }

    public class LokiLabelProvider : ILogLabelProvider
    {
        public IList<LokiLabel> GetLabels() => new List<LokiLabel>();

        public IList<string> PropertiesAsLabels => new List<string> {"AssemblyName", "level", "Path"};
        public IList<string> PropertiesToAppend => new List<string>();
        public LokiFormatterStrategy FormatterStrategy => LokiFormatterStrategy.SpecificPropertiesAsLabelsAndRestAppended;
    }
}
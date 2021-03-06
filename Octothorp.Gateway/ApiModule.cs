﻿using Autofac;
using Microsoft.AspNetCore.Hosting;
using Octothorp.Gateway.Events.Auth;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Octothorp.Gateway
{
    public class ApiModule : Module
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public ApiModule(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterLogger(builder);

            builder.RegisterType<CookieAuthEvents>();

            builder.RegisterModule<DataAccessModule>();
        }

        private void RegisterLogger(ContainerBuilder builder)
        {
            LoggerConfiguration configuration = new LoggerConfiguration();

            configuration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext();

            // TODO: Add production logger sink
            configuration.WriteTo.Console();

            Logger logger = configuration.CreateLogger();
            Log.Logger = logger;

            builder.Register(f => logger).As<ILogger>().SingleInstance();
        }
    }
}
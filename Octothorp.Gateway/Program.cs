using System;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Octothorp.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfiguration config = CreateConfig();

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddConfiguration(config);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel(options =>
                        {
                            options.Limits.MaxRequestBodySize = null;
                        });
                });

            return hostBuilder;
        }

        private static IConfiguration CreateConfig()
        {
            string? contentRootPath = Environment.GetEnvironmentVariable("contentroot");

            ConfigurationBuilder builder = new ConfigurationBuilder();

            if (!string.IsNullOrEmpty(contentRootPath))
                builder.SetBasePath(contentRootPath);

            builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("lettuceencrypt.json", optional: true, reloadOnChange: true)
                .AddJsonFile("proxy.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}

using System;
using System.IO;
using Autofac;
using LettuceEncrypt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octothorp.Gateway.Auth.Cookie;
using Octothorp.Gateway.Authorization.Handlers;
using Octothorp.Gateway.Authorization.Requirements;
using Octothorp.Gateway.Logging;
using Octothorp.Gateway.Middleware;
using Serilog;

namespace Octothorp.Gateway
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Reverse Proxy
            services.AddReverseProxy()
                .LoadFromConfig(_configuration.GetSection("ReverseProxy"));

            // LettuceEncrypt
            if (_hostEnvironment.IsProduction())
            {
                string? contentroot = Environment.GetEnvironmentVariable("contentroot") ?? _hostEnvironment.ContentRootPath;
                string certificateDirectory = Path.Combine(contentroot, "LettuceEncrypt");

                if (!Directory.Exists(certificateDirectory))
                    Directory.CreateDirectory(certificateDirectory);

                services
                    .AddLettuceEncrypt(c =>
                    {
                        bool.TryParse(_configuration["LettuceEncrypt:UseStagingServer"], out bool useStagingServer);

                        c.UseStagingServer = useStagingServer;
                    })
                    .PersistDataToDirectory(new DirectoryInfo(certificateDirectory), string.Empty);
            }

            services
                .AddControllers(options =>
                {
                    options.Filters.Add<ExceptionHandlingMiddleware>();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddHttpContextAccessor();

            services.AddRazorPages();

            // Auth
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(c =>
                {
                    c.Events = new OctoCookieAuthenticationEvents();
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("LocalMachine", policy => policy.Requirements.Add(new ZoneRequirement(ZoneRequirement.ZoneArea.LocalMachine)));
                options.AddPolicy("LocalNetwork", policy => policy.Requirements.Add(new ZoneRequirement(ZoneRequirement.ZoneArea.LocalNetwork)));
            });

            services.AddSingleton<IAuthorizationHandler, ZoneHandler>();
        }

        public void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterModule(new ApiModule(_hostEnvironment, _configuration));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = 2
            });

            app.UseStatusCodePages();

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = LogHelper.ExcludeLogging;
            });
            app.UseLoggingContext();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
                endpoints.MapControllers();
            });
        }
    }
}

using Autofac;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octothorp.Gateway.Auth.Cookie;
using Octothorp.Gateway.Authorization.Handlers;
using Octothorp.Gateway.Authorization.Requirements;
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
                options.AddPolicy("External", policy => policy.Requirements.Add(new ZoneRequirement(ZoneRequirement.ZoneArea.External)));
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

            app.UseLoggingContext();
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
                endpoints.MapControllers();
                if (!string.IsNullOrEmpty(_configuration["Certificate:ChallengeName"]))
                {
                    endpoints.Map($".well-known/acme-challenge/{_configuration["Certificate:ChallengeName"]}", async ContextBoundObject =>
                    {
                        await ContextBoundObject.Response.WriteAsync(_configuration["Certificate:ChallengeValue"]);
                    });
                }
            });
        }
    }
}

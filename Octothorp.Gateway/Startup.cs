using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            // LettuceEncrypt
            if (_hostEnvironment.IsProduction())
            {
                services.AddLettuceEncrypt(c =>
                {
                    bool.TryParse(_configuration["LettuceEncrypt:UseStagingServer"], out bool useStagingServer);

                    c.UseStagingServer = useStagingServer;
                });
            }

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("LocalMachine", policy => policy.Requirements.Add(new ZoneRequirement(ZoneRequirement.ZoneArea.LocalMachine)));
                options.AddPolicy("LocalNetwork", policy => policy.Requirements.Add(new ZoneRequirement(ZoneRequirement.ZoneArea.LocalNetwork)));
            });

            services.AddSingleton<IAuthorizationHandler, ZoneHandler>();
        }

        public void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterModule(new ApiModule(_hostEnvironment));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseStatusCodePages();

            app.UseForwardedHeaders();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
                endpoints.MapControllers();
            });
        }
    }
}

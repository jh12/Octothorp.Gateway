using Autofac;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octothorp.Gateway.Events.Auth;
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
                .AddAuthentication(options => { options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
                .AddCookie(options =>
                {
                    options.EventsType = typeof(CookieAuthEvents);
                })
                .AddDiscord(options =>
                {
                    options.ClientId = _configuration["Auth:Discord:ClientId"];
                    options.ClientSecret = _configuration["Auth:Discord:ClientSecret"];
                    options.CallbackPath = _configuration["Auth:Discord:CallbackPath"];
                });

            services.AddAuthorization();

            services
                .AddControllers(options =>
                {
                    options.Filters.Add<ExceptionHandlingMiddleware>();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddRazorPages();
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

            app.UseHttpsRedirection();

            app.UseForwardedHeaders();

            app.UseSerilogRequestLogging();

            app.UseAuthentication();

            app
                .UseRouting()
                .UseStaticFiles();
            //app.UseDefaultFiles();
            //app.UseStaticFiles();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}

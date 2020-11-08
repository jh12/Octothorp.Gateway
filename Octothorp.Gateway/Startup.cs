using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            services
                .AddAuthentication(options => { options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
                .AddCookie(options =>
                {
                    options.LoginPath = "/signin";
                    // Do not redirect to signin page when calling api
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                            return Task.CompletedTask;
                        }

                        return context.Options.Events.OnRedirectToAccessDenied(context);
                    };
                })
                .AddDiscord(options =>
                {
                    options.ClientId = _configuration["Auth:Discord:ClientId"];
                    options.ClientSecret = _configuration["Auth:Discord:ClientSecret"];
                    options.CallbackPath = _configuration["Auth:Discord:CallbackPath"];
                });

            services.AddAuthorization();

            services.AddControllers();
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

            app.UseRouting();
            app.UseStaticFiles();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();

                endpoints.MapControllers();

                endpoints.MapGet("/", async context =>
                {
                    IIdentity identity = context.User.Identity;

                    if (identity.IsAuthenticated)
                        await context.Response.WriteAsync($"Hello {identity.Name}!");
                    else
                        await context.Response.WriteAsync("Hello there stranger!");
                });
            });
        }
    }
}

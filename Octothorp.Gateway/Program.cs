using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octothorp.Gateway.Auth.Cookie;
using Octothorp.Gateway.Authorization.Handlers;
using Octothorp.Gateway.Authorization.Requirements;
using Octothorp.Gateway.Middleware;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

SetupLogger(builder);

builder.Host.UseSerilog();


IServiceCollection services = builder.Services;

services.AddSerilog(Log.Logger);
services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

services.AddScoped<LoggingContextMiddleware>();

services.AddControllers();
services.AddHttpContextAccessor();

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

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 2
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStatusCodePages();

app.UseLoggingContext();
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapControllers();

app.Run();

static void SetupLogger(WebApplicationBuilder webApplicationBuilder)
{
    LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

    if (webApplicationBuilder.Environment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.Console();
    }
    else
    {
        loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
    }

    loggerConfiguration.ReadFrom.Configuration(webApplicationBuilder.Configuration);

    loggerConfiguration
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext();

    Log.Logger = loggerConfiguration.CreateLogger();
}

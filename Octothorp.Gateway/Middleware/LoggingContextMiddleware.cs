using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Octothorp.Gateway.Middleware
{
    public class LoggingContextMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            IIdentity? identity = context.User.Identity;

            using (LogContext.PushProperty("Host", context.Request.Host.Host))
            using (LogContext.PushProperty("ClientUsername", identity?.Name))
            using (LogContext.PushProperty("IP", context.Connection.RemoteIpAddress))
                await next(context);
        }
    }

    public static class LoggingContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingContextMiddleware>();
        }
    }
}
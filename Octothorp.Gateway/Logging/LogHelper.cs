using System;
using Microsoft.AspNetCore.Http;
using Serilog.Events;

namespace Octothorp.Gateway.Logging
{
    public static class LogHelper
    {
        public static LogEventLevel ExcludeLogging(HttpContext ctx, double _, Exception ex) =>
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : IsLoggingEndpoint(ctx)
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;

        private static bool IsLoggingEndpoint(HttpContext ctx)
        {
            return ctx.Request.Host.Host.Contains("log.");
        }
    }
}
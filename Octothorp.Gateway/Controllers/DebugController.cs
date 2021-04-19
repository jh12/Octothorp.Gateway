using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Octothorp.Gateway.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        [HttpGet("version")]
        public async Task<string> GetVersion()
        {
            string? informationalVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            return informationalVersion ?? "Unknown";
        }

        [HttpGet("client/info")]
        public Task<string> GetClientInfo()
        {
            StringBuilder infoBuilder = new StringBuilder();
            
            // Server info
            infoBuilder.AppendLine("==== Server ====");
            infoBuilder.AppendFormat("Host: {0}", Request.Host);
            infoBuilder.AppendLine();
            infoBuilder.AppendLine();

            // Client IP
            infoBuilder.AppendLine("==== Client ====");
            IPAddress? remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            infoBuilder.AppendFormat("Client IP: {0}", remoteIpAddress);
            infoBuilder.AppendLine();
            infoBuilder.AppendLine();

            // Headers
            infoBuilder.AppendLine("==== Headers ====");
            foreach (var (key, value) in Request.Headers)
            {
                infoBuilder.AppendFormat("{0}: {1}", key, value);
                infoBuilder.AppendLine();
            }

            return Task.FromResult(infoBuilder.ToString());
        }
    }
}
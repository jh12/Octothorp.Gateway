using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Octothorp.Gateway.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DebugController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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
            
            // Client IP
            HttpContext httpContext = _httpContextAccessor.HttpContext!;
            IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;

            infoBuilder.AppendFormat("Client IP: {0}", remoteIpAddress);

            return Task.FromResult(infoBuilder.ToString());
        }
    }
}
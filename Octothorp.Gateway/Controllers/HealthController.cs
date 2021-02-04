using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Octothorp.Gateway.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet("version")]
        public async Task<string> GetVersion()
        {
            string? informationalVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            return informationalVersion ?? "Unknown";
        }

    }
}
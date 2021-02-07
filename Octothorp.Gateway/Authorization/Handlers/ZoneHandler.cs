using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Octothorp.Gateway.Authorization.Requirements;

namespace Octothorp.Gateway.Authorization.Handlers
{
    public class ZoneHandler : AuthorizationHandler<ZoneRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ZoneHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ZoneRequirement requirement)
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext!;
            IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            IPAddress? localIpAddress = httpContext.Connection.LocalIpAddress;

            if (requirement.Zone <= ZoneRequirement.ZoneArea.LocalMachine)
            {
                if (Equals(remoteIpAddress, localIpAddress))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            if (requirement.Zone <= ZoneRequirement.ZoneArea.LocalNetwork && remoteIpAddress != null)
            {
                if (IsPrivateAddress(remoteIpAddress))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            if (requirement.Zone <= ZoneRequirement.ZoneArea.External)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }

        private bool IsPrivateAddress(IPAddress ipAddress)
        {
            IPAddress? ipV4Address = ipAddress.AddressFamily switch
            {
                AddressFamily.InterNetwork => ipAddress,
                AddressFamily.InterNetworkV6 => ipAddress.MapToIPv4(),
                _ => null
            };

            if (ipV4Address == null)
                throw new NotImplementedException($"No support for address family {ipAddress.AddressFamily}");

            byte[] addressBytes = ipV4Address.GetAddressBytes();

            // 10.0.0.0 – 10.255.255.255
            if (addressBytes[0] == 10)
                return true;

            // 172.16.0.0 – 172.31.255.255
            if (addressBytes[0] == 192 && addressBytes[1] == 168)
                return true;

            // 192.168.0.0 – 192.168.255.255
            if (addressBytes[0] == 192 && addressBytes[1] == 168)
                return true;

            return false;
        }
    }
}
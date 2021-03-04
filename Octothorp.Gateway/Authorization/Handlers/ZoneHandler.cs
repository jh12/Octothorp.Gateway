﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NetTools;
using Octothorp.Gateway.Authorization.Requirements;

namespace Octothorp.Gateway.Authorization.Handlers
{
    public class ZoneHandler : AuthorizationHandler<ZoneRequirement>
    {
        private readonly List<IPAddressRange> _internalAddressRanges = new()
        {
            IPAddressRange.Parse("10.0.0.0/8"),
            IPAddressRange.Parse("172.16.0.0/12"),
            IPAddressRange.Parse("192.168.0.0/16"),
            IPAddressRange.Parse("fc00::/7")
        };

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
                if (_internalAddressRanges.Any(r => r.Contains(remoteIpAddress)))
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
    }
}
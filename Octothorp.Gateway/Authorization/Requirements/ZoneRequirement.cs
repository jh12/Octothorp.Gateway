using Microsoft.AspNetCore.Authorization;

namespace Octothorp.Gateway.Authorization.Requirements
{
    public class ZoneRequirement : IAuthorizationRequirement
    {
        public ZoneArea Zone { get; }

        public enum ZoneArea
        {
            LocalMachine,
            LocalNetwork,
            External
        }

        public ZoneRequirement(ZoneArea zone)
        {
            Zone = zone;
        }
    }
}

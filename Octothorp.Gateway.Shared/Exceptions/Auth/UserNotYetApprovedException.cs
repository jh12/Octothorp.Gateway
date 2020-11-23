using System.Net;

namespace Octothorp.Gateway.Shared.Exceptions.Auth
{
    public class UserNotYetApprovedException : HttpResponseException
    {
        public UserNotYetApprovedException() : base(HttpStatusCode.Forbidden, "User not yet approved for login")
        {
        }
    }
}
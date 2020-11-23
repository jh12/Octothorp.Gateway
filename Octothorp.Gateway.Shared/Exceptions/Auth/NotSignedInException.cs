using System.Net;

namespace Octothorp.Gateway.Shared.Exceptions.Auth
{
    public class NotSignedInException : HttpResponseException
    {
        public NotSignedInException() : base(HttpStatusCode.Forbidden, "User not signed in")
        {
        }
    }
}
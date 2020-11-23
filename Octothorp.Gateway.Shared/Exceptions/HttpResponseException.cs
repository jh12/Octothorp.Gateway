using System;
using System.Net;

namespace Octothorp.Gateway.Shared.Exceptions
{
    public class HttpResponseException : Exception
    {
        public readonly HttpStatusCode Status;
        public readonly string? Value;

        public HttpResponseException(HttpStatusCode status, string? value = null)
        {
            Status = status;
            Value = value;
        }
    }
}
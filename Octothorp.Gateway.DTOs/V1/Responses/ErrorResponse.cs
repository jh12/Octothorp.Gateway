using System.Net;

namespace Octothorp.Gateway.DTOs.V1.Responses
{
    public class ErrorResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public string? Response { get; set; }

        public ErrorResponse()
        {
        }

        public ErrorResponse(HttpStatusCode statusCode, string status, string? response)
        {
            StatusCode = statusCode;
            Status = status;
            Response = response;
        }
    }
}
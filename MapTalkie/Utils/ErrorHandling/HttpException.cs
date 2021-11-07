using System;
using System.Net;

namespace MapTalkie.Utils.ErrorHandling
{
    [Serializable]
    public class HttpException : Exception
    {
        public HttpException(int statusCode, string? detail)
            : base($"HTTP error: {statusCode} ({detail ?? "no message"})")
        {
            StatusCode = statusCode;
            Detail = detail ?? "no message";
        }

        public HttpException(HttpStatusCode statusCode, string detail) : this((int)statusCode, detail)
        {
        }

        public HttpException(HttpStatusCode statusCode) : this((int)statusCode, null)
        {
        }

        public int StatusCode { get; private set; }
        public string Detail { get; private set; }
    }
}
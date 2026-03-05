using System;
using System.Net;

namespace Itau.CompraProgramada.Application.Exceptions
{
    public class DomainException : Exception
    {
        public string Code { get; }
        public HttpStatusCode StatusCode { get; }

        public DomainException(string message, string code, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string message, string code) 
            : base(message, code, HttpStatusCode.NotFound) { }
    }

    public class ValidationException : DomainException
    {
        public ValidationException(string message, string code) 
            : base(message, code, HttpStatusCode.BadRequest) { }
    }
}

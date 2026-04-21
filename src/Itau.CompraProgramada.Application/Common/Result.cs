using System.Net;

namespace Itau.CompraProgramada.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public string? ErrorCode { get; }
        public HttpStatusCode StatusCode { get; }

        protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public static Result Success() => new(true);
        
        public static Result Fail(string message, string code, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            => new(false, message, code, statusCode);

        public static Result NotFound(string message, string code = "NOT_FOUND") 
            => Fail(message, code, HttpStatusCode.NotFound);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        private Result(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null, HttpStatusCode statusCode = HttpStatusCode.OK)
            : base(isSuccess, errorMessage, errorCode, statusCode)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new(true, data);
        
        public new static Result<T> Fail(string message, string code, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            => new(false, default, message, code, statusCode);

        public new static Result<T> NotFound(string message, string code = "NOT_FOUND") 
            => Fail(message, code, HttpStatusCode.NotFound);

        public static implicit operator Result<T>(T data) => Success(data);
    }
}

using Itau.CompraProgramada.Application.DTOs.Common;
using Itau.CompraProgramada.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Itau.CompraProgramada.API.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Ocorreu um erro não tratado: {Message}", exception.Message);

            var (statusCode, errorResponse) = exception switch
            {
                DomainException domainEx => ((int)domainEx.StatusCode, new ErrorResponse(domainEx.Message, domainEx.Code)),
                _ => (StatusCodes.Status500InternalServerError, new ErrorResponse("Erro interno do servidor.", "ERRO_INTERNO"))
            };

            // Custom handling for Kafka unavailability mentioned in docs
            if (exception.Message.Contains("Kafka", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Codigo = "KAFKA_INDISPONIVEL";
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}

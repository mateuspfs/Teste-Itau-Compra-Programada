using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Itau.CompraProgramada.API.Middlewares
{
    public class RequestIdMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Gera um novo UUID para cada requisição
            var requestId = Guid.NewGuid().ToString();
            
            // Adiciona o X-Request-Id no header da resposta
            context.Response.Headers.Append("X-Request-Id", requestId);
            
            // Garante o Content-Type solicitado para as rotas da API
            context.Response.OnStarting(() =>
            {
                if (context.Response.ContentType != null && context.Response.ContentType.Contains("application/json"))
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                }
                return Task.CompletedTask;
            });

            await next(context);
        }
    }
}

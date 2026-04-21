using Itau.CompraProgramada.Application.Common;
using Itau.CompraProgramada.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace Itau.CompraProgramada.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult ProcessResult<T>(Result<T> result)
        {
            if (result.IsSuccess) return Ok(result.Data);
            
            return ProcessError(result);
        }

        protected IActionResult ProcessResult(Result result)
        {
            if (result.IsSuccess) return NoContent();
            
            return ProcessError(result);
        }

        private IActionResult ProcessError(Result result)
        {
            var response = new ErrorResponse(result.ErrorMessage ?? "Erro desconhecido", result.ErrorCode ?? "ERRO_DESCONHECIDO");
            
            return result.StatusCode switch
            {
                System.Net.HttpStatusCode.NotFound => NotFound(response),
                System.Net.HttpStatusCode.BadRequest => BadRequest(response),
                System.Net.HttpStatusCode.Unauthorized => Unauthorized(response),
                System.Net.HttpStatusCode.Forbidden => Forbid(),
                _ => StatusCode((int)result.StatusCode, response)
            };
        }
    }
}

using Itau.CompraProgramada.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;

namespace Itau.CompraProgramada.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Controle Manual e Teste do Motor de Compra")]
    public class MotorCompraController(IMotorCompraEngine motorCompraEngine) : ControllerBase
    {
        /// <summary>
        /// Dispara manualmente o motor de processamento diário.
        /// </summary>
        /// <param name="data">Data para simular o processamento (formato YYYY-MM-DD). Se ignorado, usa a data atual.</param>
        /// <returns>Confirmação de disparo.</returns>
        [HttpPost("executar-teste")]
        [SwaggerOperation(Summary = "Disparar processamento diário", Description = "Força a execução da lógica do motor (fração, rebalanceamento e aporte) para uma data específica.")]
        [SwaggerResponse(200, "Processamento disparado com sucesso")]
        public async Task<IActionResult> ExecutarTeste([FromQuery] string? data)
        {
            if (!DateTime.TryParse(data, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataProcessamento))
                dataProcessamento = DateTime.UtcNow;

            var result = await motorCompraEngine.ExecutarProcessamentoDiarioAsync(dataProcessamento);
            
            return Ok(result);
        }
    }
}

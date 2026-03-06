using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Itau.CompraProgramada.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController(
        IClienteService clienteService,
        ICarteiraService carteiraService) : ControllerBase
    {
        [HttpGet("{clienteId}/carteira")]
        public async Task<IActionResult> ObterCarteira(long clienteId)
        {
            var response = await carteiraService.ObterCarteiraPorClienteAsync(clienteId);
            return Ok(response);
        }

        [HttpPost("adesao")]
        public async Task<IActionResult> AderirAoProduto([FromBody] AdesaoClienteRequest request)
        {
            var response = await clienteService.AderirAoProdutoAsync(request);
            return CreatedAtAction(nameof(AderirAoProduto), new { clienteId = response.ClienteId }, response);
        }

        [HttpPost("{clienteId}/saida")]
        public async Task<IActionResult> SairDoProduto(long clienteId)
        {
            var response = await clienteService.SairDoProdutoAsync(clienteId);
            return Ok(response);
        }

        [HttpPut("{clienteId}/valor-mensal")]
        public async Task<IActionResult> AlterarValorMensal(long clienteId, [FromBody] AlterarValorMensalRequest request)
        {
            var response = await clienteService.AlterarValorMensalAsync(clienteId, request);
            return Ok(response);
        }
    }
}

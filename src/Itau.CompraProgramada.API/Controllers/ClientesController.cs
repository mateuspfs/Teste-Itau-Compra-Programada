using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Itau.CompraProgramada.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gerenciamento de Clientes e Adesões ao Produto")]
    public class ClientesController(
        IClienteService clienteService,
        ICarteiraService carteiraService) : ControllerBase
    {
        /// <summary>
        /// Consulta a carteira de ações de um cliente.
        /// </summary>
        /// <param name="clienteId">ID do cliente.</param>
        /// <returns>Dados da custódia atual do cliente.</returns>
        [HttpGet("{clienteId}/carteira")]
        [SwaggerOperation(Summary = "Obter carteira do cliente", Description = "Retorna a lista de ativos e quantidades que o cliente possui atualmente.")]
        [SwaggerResponse(200, "Carteira retornada com sucesso", typeof(CarteiraResponse))]
        [SwaggerResponse(404, "Cliente não encontrado")]
        public async Task<IActionResult> ObterCarteira(long clienteId)
        {
            var response = await carteiraService.ObterCarteiraPorClienteAsync(clienteId);
            return Ok(response);
        }

        /// <summary>
        /// Realiza a adesão de um novo cliente ao produto de Compra Programada.
        /// </summary>
        /// <param name="request">Dados de cadastro e aporte inicial.</param>
        /// <returns>Confirmação de adesão.</returns>
        [HttpPost("adesao")]
        [SwaggerOperation(Summary = "Aderir ao produto", Description = "Cadastra um cliente e define o valor de aporte mensal para a carteira Top Five.")]
        [SwaggerResponse(201, "Adesão realizada com sucesso", typeof(AdesaoClienteResponse))]
        [SwaggerResponse(400, "Dados de entrada inválidos")]
        public async Task<IActionResult> AderirAoProduto([FromBody] AdesaoClienteRequest request)
        {
            var response = await clienteService.AderirAoProdutoAsync(request);
            return CreatedAtAction(nameof(AderirAoProduto), new { clienteId = response.ClienteId }, response);
        }

        /// <summary>
        /// Cancela a participação do cliente no produto.
        /// </summary>
        /// <param name="clienteId">ID do cliente.</param>
        /// <returns>Status do cancelamento.</returns>
        [HttpPost("{clienteId}/saida")]
        [SwaggerOperation(Summary = "Sair do produto", Description = "Desativa a conta do cliente para novos aportes e rebalanceamentos.")]
        [SwaggerResponse(200, "Saída realizada com sucesso")]
        [SwaggerResponse(404, "Cliente não encontrado")]
        public async Task<IActionResult> SairDoProduto(long clienteId)
        {
            var response = await clienteService.SairDoProdutoAsync(clienteId);
            return Ok(response);
        }

        /// <summary>
        /// Altera o valor de investimento mensal do cliente.
        /// </summary>
        /// <param name="clienteId">ID do cliente.</param>
        /// <param name="request">Novo valor desejado.</param>
        /// <returns>Dados da alteração confirmada.</returns>
        [HttpPut("{clienteId}/valor-mensal")]
        [SwaggerOperation(Summary = "Alterar valor mensal", Description = "Atualiza o valor que será utilizado nos próximos ciclos de investimento (dias 5, 15 ou 25).")]
        [SwaggerResponse(200, "Valor alterado com sucesso", typeof(AlterarValorMensalResponse))]
        [SwaggerResponse(400, "Valor inválido")]
        public async Task<IActionResult> AlterarValorMensal(long clienteId, [FromBody] AlterarValorMensalRequest request)
        {
            var response = await clienteService.AlterarValorMensalAsync(clienteId, request);
            return Ok(response);
        }

        /// <summary>
        /// Consulta a rentabilidade consolidada do cliente.
        /// </summary>
        /// <param name="clienteId">ID do cliente.</param>
        /// <returns>Resumo de P/L e evolução da carteira.</returns>
        [HttpGet("{clienteId}/rentabilidade")]
        [SwaggerOperation(Summary = "Obter rentabilidade", Description = "Calcula o ganho/perda de capital comparando o valor total aplicado com o valor de mercado atual.")]
        [SwaggerResponse(200, "Rentabilidade calculada com sucesso", typeof(RentabilidadeResponse))]
        [SwaggerResponse(404, "Cliente não encontrado")]
        public async Task<IActionResult> ObterRentabilidade(long clienteId)
        {
            var response = await clienteService.ObterRentabilidadeAsync(clienteId);
            return Ok(response);
        }
    }
}

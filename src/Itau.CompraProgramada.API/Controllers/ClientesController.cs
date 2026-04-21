using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Itau.CompraProgramada.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gerenciamento de Clientes e Adesões ao Produto")]
    public class ClientesController(
        IClienteService clienteService,
        ICarteiraService carteiraService) : BaseController
    {
        [HttpGet("{clienteId}/carteira")]
        [SwaggerOperation(Summary = "Obter carteira do cliente", Description = "Retorna a lista de ativos e quantidades que o cliente possui atualmente.")]
        [SwaggerResponse(200, "Carteira retornada com sucesso", typeof(CarteiraResponse))]
        [SwaggerResponse(404, "Cliente não encontrado")]
        public async Task<IActionResult> ObterCarteira(long clienteId)
        {
            return ProcessResult(await carteiraService.ObterCarteiraPorClienteAsync(clienteId));
        }

        /// <summary>
        /// Lista todos os clientes cadastrados com paginação.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Listar clientes paginado", Description = "Retorna os clientes cadastrados no sistema com suporte a paginação e ordenação.")]
        [SwaggerResponse(200, "Lista de clientes retornada com sucesso", typeof(ResultadoPaginado<AdesaoClienteResponse>))]
        public async Task<IActionResult> ObterTodos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 10, [FromQuery] bool ordemDesc = true)
        {
            return ProcessResult(await clienteService.ObterTodosPaginaAsync(pagina, tamanhoPagina, ordemDesc));
        }

        /// <summary>
        /// Obtém o resumo de clientes para o dashboard.
        /// </summary>
        [HttpGet("resumo")]
        [SwaggerOperation(Summary = "Resumo de clientes", Description = "Retorna métricas consolidadas de clientes para o dashboard.")]
        [SwaggerResponse(200, "Resumo retornado com sucesso", typeof(ClienteResumoResponse))]
        public async Task<IActionResult> ObterResumo()
        {
            return ProcessResult(await clienteService.ObterResumoDashboardAsync());
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
            var result = await clienteService.AderirAoProdutoAsync(request);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(AderirAoProduto), new { clienteId = result.Data!.ClienteId }, result.Data);
            }
            return ProcessResult(result);
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
            return ProcessResult(await clienteService.SairDoProdutoAsync(clienteId));
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
            return ProcessResult(await clienteService.AlterarValorMensalAsync(clienteId, request));
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
            return ProcessResult(await clienteService.ObterRentabilidadeAsync(clienteId));
        }
    }
}

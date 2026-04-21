using Itau.CompraProgramada.Application.DTOs.Admin;
using Itau.CompraProgramada.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Itau.CompraProgramada.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Operações Administrativas e Gestão de Carteiras Recomendadas")]
    public class AdminController(IAdminService adminService) : BaseController
    {
        /// <summary>
        /// Cadastra ou atualiza uma cesta recomendada de ativos.
        /// </summary>
        /// <param name="request">Lista de ativos e seus pesos percentuais.</param>
        /// <returns>Dados da cesta criada.</returns>
        [HttpPost("cesta")]
        [SwaggerOperation(Summary = "Configurar cesta recomendada", Description = "Define a composição da carteira 'Top Five' que será utilizada nos rebalanceamentos.")]
        [SwaggerResponse(201, "Cesta configurada com sucesso", typeof(CestaCadastroResponse))]
        [SwaggerResponse(400, "Soma dos pesos diferente de 100%")]
        public async Task<IActionResult> CadastrarAlterarCesta([FromBody] CestaRequest request)
        {
            var result = await adminService.CadastrarAlterarCestaAsync(request);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(CadastrarAlterarCesta), new { cestaId = result.Data!.CestaId }, result.Data);
            }
            return ProcessResult(result);
        }

        /// <summary>
        /// Consulta a composição da cesta recomendada ativa no momento.
        /// </summary>
        /// <returns>Composição atual da Top Five.</returns>
        [HttpGet("cesta/atual")]
        [SwaggerOperation(Summary = "Obter cesta atual", Description = "Retorna os ativos e pesos vigentes no sistema.")]
        [SwaggerResponse(200, "Cesta retornada com sucesso", typeof(CestaDetalhesResponse))]
        public async Task<IActionResult> ObterCestaAtual()
        {
            return ProcessResult(await adminService.ObterCestaAtualAsync());
        }

        /// <summary>
        /// Consulta o histórico de todas as cestas já configuradas.
        /// </summary>
        /// <returns>Lista de cestas históricas.</returns>
        [HttpGet("cesta/historico")]
        [SwaggerOperation(Summary = "Listar histórico de cestas", Description = "Retorna todas as composições de carteira que já foram recomendadas pelo sistema.")]
        [SwaggerResponse(200, "Histórico retornado com sucesso", typeof(IEnumerable<CestaHistoricoResponse>))]
        public async Task<IActionResult> ObterHistoricoCestas()
        {
            var result = await adminService.ObterHistoricoCestasAsync();
            if (result.IsSuccess)
            {
                return Ok(new { cestas = result.Data });
            }
            return ProcessResult(result);
        }

        /// <summary>
        /// Consulta o saldo de ativos mantidos na Conta Master.
        /// </summary>
        /// <returns>Lista de ativos e quantidades da conta master.</returns>
        [HttpGet("conta-master/custodia")]
        [SwaggerOperation(Summary = "Obter custódia master", Description = "Retorna o saldo residual de frações de ações decorrentes do processo de distribuição entre clientes.")]
        [SwaggerResponse(200, "Custódia master retornada com sucesso", typeof(CustodiaMasterResponse))]
        public async Task<IActionResult> ObterCustodiaMaster()
        {
            return ProcessResult(await adminService.ObterCustodiaMasterAsync());
        }
    }
}

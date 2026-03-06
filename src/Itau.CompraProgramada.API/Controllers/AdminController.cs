using Itau.CompraProgramada.Application.DTOs.Admin;
using Itau.CompraProgramada.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Itau.CompraProgramada.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(IAdminService adminService) : ControllerBase
    {
        [HttpPost("cesta")]
        public async Task<IActionResult> CadastrarAlterarCesta([FromBody] CestaRequest request)
        {
            var response = await adminService.CadastrarAlterarCestaAsync(request);
            return CreatedAtAction(nameof(CadastrarAlterarCesta), new { cestaId = response.CestaId }, response);
        }

        [HttpGet("cesta/atual")]
        public async Task<IActionResult> ObterCestaAtual()
        {
            var response = await adminService.ObterCestaAtualAsync();
            return Ok(response);
        }

        [HttpGet("cesta/historico")]
        public async Task<IActionResult> ObterHistoricoCestas()
        {
            var response = await adminService.ObterHistoricoCestasAsync();
            return Ok(new { cestas = response });
        }
    }
}

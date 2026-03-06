using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Clientes;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface ICarteiraService
    {
        Task<CarteiraResponse> ObterCarteiraPorClienteAsync(long clienteId);
    }
}

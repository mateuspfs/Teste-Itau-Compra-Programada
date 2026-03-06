using System.Threading.Tasks;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IIRService
    {
        Task ProcessarIRDedoDuroAsync(long clienteId, string ticker, decimal valorOperacao, string tipoOperacao, int quantidade, decimal precoUnitario);
        Task ProcessarIRVendaMensalAsync(long clienteId, int mes, int ano);
    }
}

using System.Threading.Tasks;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IRebalanceamentoService
    {
        /// <summary>
        /// Dispara o processo de rebalanceamento para todos os clientes ativos devido à mudança de cesta.
        /// </summary>
        Task ProcessarRebalanceamentoPorMudancaCestaAsync(long cestaAnteriorId, long novaCestaId);
    }
}

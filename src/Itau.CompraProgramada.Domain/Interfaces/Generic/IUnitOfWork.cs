using Itau.CompraProgramada.Domain.Interfaces.Respositories;

namespace Itau.CompraProgramada.Domain.Interfaces.Generic
{
    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository Clientes { get; }
        IContaGraficaRepository Contas { get; }
        ICustodiaRepository Custodias { get; }
        ICestaRecomendacaoRepository Cestas { get; }
        IItemCestaRepository ItensCesta { get; }
        ICotacaoRepository Cotacoes { get; }
        IOrdemCompraRepository Ordens { get; }
        IDistribuicaoRepository Distribuicoes { get; }
        IEventoIRRepository EventosIR { get; }
        ILogRepository Logs { get; }
        
        Task<int> CommitAsync();
    }
}

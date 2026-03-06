using Itau.CompraProgramada.Domain.Interfaces.Generic;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;

namespace Itau.CompraProgramada.Infrastructure.Data
{
    public class UnitOfWork(
        ApplicationDbContext context,
        IClienteRepository clientes,
        IContaGraficaRepository contas,
        ICustodiaRepository custodias,
        ICestaRecomendacaoRepository cestas,
        IItemCestaRepository itensCesta,
        ICotacaoRepository cotacoes,
        IOrdemCompraRepository ordens,
        IDistribuicaoRepository distribuicoes,
        IEventoIRRepository eventosIR,
        ILogRepository logs) : IUnitOfWork
    {
        private readonly ApplicationDbContext _context = context;
        private bool _disposed;

        public IClienteRepository Clientes => clientes;
        public IContaGraficaRepository Contas => contas;
        public ICustodiaRepository Custodias => custodias;
        public ICestaRecomendacaoRepository Cestas => cestas;
        public IItemCestaRepository ItensCesta => itensCesta;
        public ICotacaoRepository Cotacoes => cotacoes;
        public IOrdemCompraRepository Ordens => ordens;
        public IDistribuicaoRepository Distribuicoes => distribuicoes;
        public IEventoIRRepository EventosIR => eventosIR;
        public ILogRepository Logs => logs;

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _context.Dispose();                
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

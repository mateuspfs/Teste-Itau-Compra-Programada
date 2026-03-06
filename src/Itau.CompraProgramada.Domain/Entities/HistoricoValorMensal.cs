using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class HistoricoValorMensal : Entity
    {
        public long ClienteId { get; private set; }
        public decimal ValorAnterior { get; private set; }
        public decimal ValorNovo { get; private set; }
        public DateTime DataAlteracao { get; private set; }

        public virtual Cliente Cliente { get; private set; }

        protected HistoricoValorMensal() { }

        public HistoricoValorMensal(long clienteId, decimal valorAnterior, decimal valorNovo)
        {
            ClienteId = clienteId;
            ValorAnterior = valorAnterior;
            ValorNovo = valorNovo;
            DataAlteracao = DateTime.UtcNow;
        }
    }
}

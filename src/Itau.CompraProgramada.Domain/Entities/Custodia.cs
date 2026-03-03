using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class Custodia : Entity
    {
        public long ContaGraficaId { get; private set; }
        public string Ticker { get; private set; }
        public int Quantidade { get; private set; }
        public decimal PrecoMedio { get; private set; }
        public DateTime DataUltimaAtualizacao { get; private set; }

        public virtual ContaGrafica ContaGrafica { get; private set; }

        protected Custodia() { }

        public Custodia(long contaGraficaId, string ticker, int quantidade, decimal precoMedio)
        {
            ContaGraficaId = contaGraficaId;
            Ticker = ticker;
            Quantidade = quantidade;
            PrecoMedio = precoMedio;
            DataUltimaAtualizacao = DateTime.UtcNow;
        }
    }
}

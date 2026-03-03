using System;
using Itau.CompraProgramada.Domain.Enums;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class Rebalanceamento : Entity
    {
        public long ClienteId { get; private set; }
        public RebalanceamentoTipo Tipo { get; private set; }
        public string TickerVendido { get; private set; }
        public string TickerComprado { get; private set; }
        public decimal ValorVenda { get; private set; }
        public DateTime DataRebalanceamento { get; private set; }

        public virtual Cliente Cliente { get; private set; }

        protected Rebalanceamento() { }

        public Rebalanceamento(long clienteId, RebalanceamentoTipo tipo, string tickerVendido, string tickerComprado, decimal valorVenda)
        {
            ClienteId = clienteId;
            Tipo = tipo;
            TickerVendido = tickerVendido;
            TickerComprado = tickerComprado;
            ValorVenda = valorVenda;
            DataRebalanceamento = DateTime.UtcNow;
        }
    }
}

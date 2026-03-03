using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class Cotacao : Entity
    {
        public DateTime DataPregao { get; private set; }
        public string Ticker { get; private set; }
        public decimal PrecoAbertura { get; private set; }
        public decimal PrecoFechamento { get; private set; }
        public decimal PrecoMaximo { get; private set; }
        public decimal PrecoMinimo { get; private set; }

        protected Cotacao() { }

        public Cotacao(DateTime dataPregao, string ticker, decimal abertura, decimal fechamento, decimal maximo, decimal minimo)
        {
            DataPregao = dataPregao;
            Ticker = ticker;
            PrecoAbertura = abertura;
            PrecoFechamento = fechamento;
            PrecoMaximo = maximo;
            PrecoMinimo = minimo;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Itau.CompraProgramada.Application.DTOs.Clientes
{
    public class CarteiraResponse
    {
        public long ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string ContaGrafica { get; set; } = string.Empty;
        public DateTime DataConsulta { get; set; }
        public CarteiraResumoDTO Resumo { get; set; } = new();
        public List<AtivoCarteiraDTO> Ativos { get; set; } = new();
    }

    public class CarteiraResumoDTO
    {
        public decimal ValorTotalInvestido { get; set; }
        public decimal ValorAtualCarteira { get; set; }
        public decimal PLTotal { get; set; }
        public decimal RentabilidadePercentual { get; set; }
    }

    public class AtivoCarteiraDTO
    {
        public string Ticker { get; set; } = null!;
        public int Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal CotacaoAtual { get; set; }
        public decimal ValorAtual { get; set; }
        public decimal PL { get; set; }
        public decimal PLPercentual { get; set; }
        public decimal ComposicaoCarteira { get; set; }
    }
}

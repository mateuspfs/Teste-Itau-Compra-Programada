using System.Collections.Generic;

namespace Itau.CompraProgramada.Application.DTOs.Clientes
{
    public class CarteiraResponse
    {
        public decimal ValorInvestidoTotal { get; set; }
        public decimal ValorAtualTotal { get; set; }
        public decimal PLTotal { get; set; }
        public decimal RentabilidadePercentual { get; set; }
        public List<AtivoCarteiraDTO> Ativos { get; set; } = new();
    }

    public class AtivoCarteiraDTO
    {
        public string Ticker { get; set; } = null!;
        public int Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal CotacaoAtual { get; set; }
        public decimal ValorAtual { get; set; }
        public decimal PL { get; set; }
        public decimal ComposicaoPercentual { get; set; }
    }
}

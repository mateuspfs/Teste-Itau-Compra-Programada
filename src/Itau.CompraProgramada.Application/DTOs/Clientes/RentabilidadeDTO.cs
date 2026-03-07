using System;
using System.Collections.Generic;

namespace Itau.CompraProgramada.Application.DTOs.Clientes
{
    public class RentabilidadeResponse
    {
        public long ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataConsulta { get; set; }
        public RentabilidadeResumoDTO Rentabilidade { get; set; } = new();
        public List<HistoricoAporteDTO> HistoricoAportes { get; set; } = new();
        public List<EvolucaoCarteiraDTO> EvolucaoCarteira { get; set; } = new();
    }

    public class RentabilidadeResumoDTO
    {
        public decimal ValorTotalInvestido { get; set; }
        public decimal ValorAtualCarteira { get; set; }
        public decimal PLTotal { get; set; }
        public decimal RentabilidadePercentual { get; set; }
    }

    public class HistoricoAporteDTO
    {
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string Parcela { get; set; } = string.Empty;
    }

    public class EvolucaoCarteiraDTO
    {
        public DateTime Data { get; set; }
        public decimal ValorCarteira { get; set; }
        public decimal ValorInvestido { get; set; }
        public decimal Rentabilidade { get; set; }
    }
}

using System;
using System.Text.Json.Serialization;

namespace Itau.CompraProgramada.Application.DTOs.Clientes
{
    public class ClienteResumoResponse
    {
        [JsonPropertyName("totalAtivos")]
        public int TotalAtivos { get; set; }

        [JsonPropertyName("totalValorMensal")]
        public decimal TotalValorMensal { get; set; }

        [JsonPropertyName("totalResiduoMaster")]
        public decimal TotalResiduoMaster { get; set; }

        [JsonPropertyName("ultimosClientes")]
        public IEnumerable<UltimoClienteDashboardDTO> UltimosClientes { get; set; } = new List<UltimoClienteDashboardDTO>();

        [JsonPropertyName("itensMaster")]
        public IEnumerable<ItemCustodiaMasterResumoDTO> ItensMaster { get; set; } = new List<ItemCustodiaMasterResumoDTO>();

        [JsonPropertyName("dataReferencia")]
        public DateTime DataReferencia { get; set; } = DateTime.UtcNow;
    }

    public class UltimoClienteDashboardDTO
    {
        public long ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal ValorMensal { get; set; }
        public string? NumeroConta { get; set; }
    }

    public class ItemCustodiaMasterResumoDTO
    {
        public string Ticker { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal ValorAtual { get; set; }
    }
}

using System.Collections.Generic;

namespace Itau.CompraProgramada.Application.DTOs.Admin
{
    public class CustodiaMasterResponse
    {
        public ContaMasterDTO ContaMaster { get; set; } = new();
        public List<ItemCustodiaMasterDTO> Custodia { get; set; } = new();
        public decimal ValorTotalResiduo { get; set; }
    }

    public class ContaMasterDTO
    {
        public long Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    public class ItemCustodiaMasterDTO
    {
        public string Ticker { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorAtual { get; set; }
        public string Origem { get; set; } = string.Empty;
    }
}

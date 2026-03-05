using System;
using System.Collections.Generic;

namespace Itau.CompraProgramada.Application.DTOs.Admin
{
    public class CestaRequest
    {
        public string Nome { get; set; } = string.Empty;
        public List<ItemCestaDTO> Itens { get; set; } = new();
    }

    public class CestaCadastroResponse
    {
        public long CestaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<ItemCestaDTO> Itens { get; set; } = new();
        public bool RebalanceamentoDisparado { get; set; }
        public CestaResumoDTO? CestaAnteriorDesativada { get; set; }
        public List<string>? AtivosRemovidos { get; set; }
        public List<string>? AtivosAdicionados { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class CestaDetalhesResponse
    {
        public long CestaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<ItemCestaDetalhesDTO> Itens { get; set; } = new();
    }

    public class CestaHistoricoResponse
    {
        public long CestaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataDesativacao { get; set; }
        public List<ItemCestaDTO> Itens { get; set; } = new();
    }

    public class ItemCestaDTO
    {
        public string Ticker { get; set; } = string.Empty;
        public decimal Percentual { get; set; }
    }

    public class ItemCestaDetalhesDTO : ItemCestaDTO
    {
        public decimal? CotacaoAtual { get; set; }
    }

    public class CestaResumoDTO
    {
        public long CestaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataDesativacao { get; set; }
    }
}

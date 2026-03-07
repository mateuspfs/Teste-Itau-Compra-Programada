using System;
using System.Collections.Generic;

namespace Itau.CompraProgramada.Application.DTOs.Admin
{
    public class MotorCompraResponse
    {
        public DateTime DataExecucao { get; set; }
        public int TotalClientes { get; set; }
        public decimal TotalConsolidado { get; set; }
        public List<MotorOrdemCompraDTO> OrdensCompra { get; set; } = new();
        public List<MotorDistribuicaoDTO> Distribuicoes { get; set; } = new();
        public List<MotorResiduoMasterDTO> ResiduosCustMaster { get; set; } = new();
        public int EventosIRPublicados { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class MotorOrdemCompraDTO
    {
        public string Ticker { get; set; } = null!;
        public int QuantidadeTotal { get; set; }
        public List<MotorOrdemDetalheDTO> Detalhes { get; set; } = new();
        public decimal PrecoUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class MotorOrdemDetalheDTO
    {
        public string Tipo { get; set; } = null!;
        public string Ticker { get; set; } = null!;
        public int Quantidade { get; set; }
    }

    public class MotorDistribuicaoDTO
    {
        public long ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorAporte { get; set; }
        public List<MotorAtivoDTO> Ativos { get; set; } = new();
    }

    public class MotorAtivoDTO
    {
        public string Ticker { get; set; } = null!;
        public int Quantidade { get; set; }
    }

    public class MotorResiduoMasterDTO
    {
        public string Ticker { get; set; } = null!;
        public int Quantidade { get; set; }
    }
}

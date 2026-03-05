using System;

namespace Itau.CompraProgramada.Application.DTOs.Clientes
{
    public class AdesaoClienteRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal ValorMensal { get; set; }
    }

    public class AdesaoClienteResponse
    {
        public long ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal ValorMensal { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataAdesao { get; set; }
        public ContaGraficaDTO ContaGrafica { get; set; } = null!;
    }

    public class ContaGraficaDTO
    {
        public long Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}

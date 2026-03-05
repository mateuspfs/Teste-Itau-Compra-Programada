using System;

namespace Itau.CompraProgramada.Application.DTOs.Clientes
{
    public class SaidaProdutoResponse
    {
        public long ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataSaida { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class AlterarValorMensalRequest
    {
        public decimal NovoValorMensal { get; set; }
    }

    public class AlterarValorMensalResponse
    {
        public long ClienteId { get; set; }
        public decimal ValorMensalAnterior { get; set; }
        public decimal ValorMensalNovo { get; set; }
        public DateTime DataAlteracao { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}

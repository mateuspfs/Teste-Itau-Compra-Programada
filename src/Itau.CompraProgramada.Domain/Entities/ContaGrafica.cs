using System;
using Itau.CompraProgramada.Domain.Enums;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class ContaGrafica : Entity
    {
        public long ClienteId { get; private set; }
        public string NumeroConta { get; private set; }
        public ContaTipo Tipo { get; private set; }

        public virtual Cliente Cliente { get; private set; }

        protected ContaGrafica() { }

        public ContaGrafica(long clienteId, string numeroConta, ContaTipo tipo)
        {
            ClienteId = clienteId;
            NumeroConta = numeroConta;
            Tipo = tipo;
            DataCriacao = DateTime.UtcNow;
        }
    }
}

using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class Cliente : Entity
    {
        public string Nome { get; private set; }
        public string CPF { get; private set; }
        public string Email { get; private set; }
        public decimal ValorMensal { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime DataAdesao { get; private set; }

        protected Cliente() { }

        public Cliente(string nome, string cpf, string email, decimal valorMensal)
        {
            if (!ValueObjects.Cpf.Validar(cpf))
                throw new ArgumentException("CPF invalido.");

            Nome = nome;
            CPF = new ValueObjects.Cpf(cpf).Valor;
            Email = email;
            ValorMensal = valorMensal;
            Ativo = true;
            DataAdesao = DateTime.UtcNow;
        }

        public void Desativar() => Ativo = false;
        public void AlterarValorMensal(decimal novoValor) => ValorMensal = novoValor;
    }
}

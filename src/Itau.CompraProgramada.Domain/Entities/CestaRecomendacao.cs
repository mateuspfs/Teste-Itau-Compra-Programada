using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class CestaRecomendacao : Entity
    {
        public string Nome { get; private set; }
        public bool Ativa { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataDesativacao { get; private set; }

        protected CestaRecomendacao() { }

        public CestaRecomendacao(string nome)
        {
            Nome = nome;
            Ativa = true;
            DataCriacao = DateTime.UtcNow;
        }

        public void Desativar()
        {
            Ativa = false;
            DataDesativacao = DateTime.UtcNow;
        }
    }
}

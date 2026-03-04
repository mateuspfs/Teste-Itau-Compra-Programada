using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public class Log : Entity
    {
        public string Nivel { get; private set; }
        public string Mensagem { get; private set; }
        public string? Excecao { get; private set; }
        public string? Origem { get; private set; }

        protected Log() { }

        public Log(string nivel, string mensagem, string? excecao = null, string? origem = null)
        {
            Nivel = nivel;
            Mensagem = mensagem;
            Excecao = excecao;
            Origem = origem;
            DataCriacao = DateTime.UtcNow;
        }
    }
}

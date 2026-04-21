namespace Itau.CompraProgramada.Application.DTOs.Common
{
    public class ErrorResponse
    {
        public string Mensagem { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;

        public ErrorResponse() { }

        public ErrorResponse(string mensagem, string codigo)
        {
            Mensagem = mensagem;
            Codigo = codigo;
        }
    }
}

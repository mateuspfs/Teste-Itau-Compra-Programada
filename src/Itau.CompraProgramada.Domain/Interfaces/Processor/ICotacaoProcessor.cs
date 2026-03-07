namespace Itau.CompraProgramada.Domain.Interfaces.Processor
{
    public interface ICotacaoProcessor
    {
        Task<int> ProcessarArquivoAsync(string filePath);
    }
}

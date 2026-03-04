namespace Itau.CompraProgramada.Domain.Interfaces.Imports
{
    public interface ICotacaoImport
    {
        Task<int> ProcessarArquivoAsync(string filePath);
    }
}

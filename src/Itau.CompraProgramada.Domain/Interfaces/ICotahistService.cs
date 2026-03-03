using System.Threading.Tasks;

namespace Itau.CompraProgramada.Domain.Interfaces
{
    public interface ICotahistService
    {
        Task<int> ProcessarArquivoAsync(string filePath);
    }
}

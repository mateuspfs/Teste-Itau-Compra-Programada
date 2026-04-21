using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Generic;

namespace Itau.CompraProgramada.Domain.Interfaces.Respositories
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<Cliente?> GetByCpfAsync(string cpf);
        Task<IEnumerable<Cliente>> GetAtivosAsync();
        Task<DashboardData> ObterResumoAsync();
    }

    public class DashboardData
    {
        public int TotalAtivos { get; set; }
        public decimal TotalValorMensal { get; set; }
        public decimal ValorResiduoMaster { get; set; }
        public IEnumerable<dynamic> ItensMaster { get; set; } = new List<dynamic>();
    }
}

using Dapper;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class ClienteRepository(ApplicationDbContext db) : GenericRepository<Cliente>(db), IClienteRepository
    {
        public async Task<Cliente?> GetByCpfAsync(string cpf)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CPF == cpf);
        }

        public async Task<IEnumerable<Cliente>> GetAtivosAsync()
        {
            return await _dbSet.Where(c => c.Ativo).ToListAsync();
        }

        public async Task<DashboardData> ObterResumoAsync()
        {
            var connection = _context.Database.GetDbConnection();
            
            const string sql = @"
                -- 1. Métricas Globais
                SELECT 
                    (SELECT COUNT(*) FROM Clientes WHERE Ativo = 1) as TotalAtivos,
                    (SELECT COALESCE(SUM(ValorMensal), 0) FROM Clientes WHERE Ativo = 1) as TotalValorMensal,
                    (SELECT COALESCE(SUM(c.Quantidade * co.PrecoFechamento), 0)
                     FROM Custodias c
                     INNER JOIN ContasGraficas cg ON c.ContaGraficaId = cg.Id
                     LEFT JOIN (
                         SELECT Ticker, PrecoFechamento, ROW_NUMBER() OVER(PARTITION BY Ticker ORDER BY DataPregao DESC) as rn
                         FROM Cotacoes
                     ) co ON c.Ticker = co.Ticker AND co.rn = 1
                     WHERE cg.Tipo = 2) as ValorResiduoMaster;

                -- 2. Itens da Master (Composição)
                SELECT 
                    c.Ticker, 
                    c.Quantidade, 
                    COALESCE(c.Quantidade * co.PrecoFechamento, 0) as ValorAtual
                FROM Custodias c
                INNER JOIN ContasGraficas cg ON c.ContaGraficaId = cg.Id
                LEFT JOIN (
                    SELECT Ticker, PrecoFechamento, ROW_NUMBER() OVER(PARTITION BY Ticker ORDER BY DataPregao DESC) as rn
                    FROM Cotacoes
                ) co ON c.Ticker = co.Ticker AND co.rn = 1
                WHERE cg.Tipo = 2 -- MASTER
                AND c.Quantidade > 0;
            ";

            using var multi = await connection.QueryMultipleAsync(sql);
            
            var stats = await multi.ReadSingleAsync();
            var itensMaster = await multi.ReadAsync<dynamic>();
            
            return new DashboardData
            {
                TotalAtivos = (int)(stats.TotalAtivos ?? 0),
                TotalValorMensal = (decimal)(stats.TotalValorMensal ?? 0m),
                ValorResiduoMaster = (decimal)(stats.ValorResiduoMaster ?? 0m),
                ItensMaster = itensMaster
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Itau.CompraProgramada.Domain.Entities;
using Microsoft.Extensions.Logging;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Processor;

namespace Itau.CompraProgramada.Infrastructure.Data
{
    public interface IInicializadorBanco
    {
        Task InicializarAsync();
    }

    public class InicializadorBanco(
        ApplicationDbContext context,
        ICotacaoProcessor cotacaoService,
        ILogRepository logRepository,
        ILogger<InicializadorBanco> logger) : IInicializadorBanco
    {
        public async Task InicializarAsync()
        {
            try
            {
                logger.LogInformation("Executando migrations pendentes");
                await context.Database.MigrateAsync();

                await LogAsync("Information", "Banco de dados inicializado/migrado com sucesso.");

                await SeedMasterDataAsync();
                await SeedCotacoesAsync();

                await LogAsync("Information", "Inicialização do banco de dados concluída com sucesso.");
                logger.LogInformation("Inicialização concluída.");
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "Erro durante a inicialização do banco de dados", ex.ToString());
                logger.LogError(ex, "Erro ao inicializar o banco.");
                throw;
            }
        }

        private async Task SeedMasterDataAsync()
        {
            var contaMaster = await context.ContasGraficas.FirstOrDefaultAsync(c => c.Tipo == ContaTipo.MASTER);
            if (contaMaster == null)
            {
                // Garantir existência da Conta Master e Cliente Master no sistema
                logger.LogInformation("Conta Master não encontrada. Criando registros mestre...");
                var cpfMaster = "000000000";
                var clienteMaster = await context.Clientes.FirstOrDefaultAsync(c => c.CPF == cpfMaster);
                if (clienteMaster == null)
                {
                    logger.LogInformation("Inserindo Cliente Master via SQL...");
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO Clientes (Nome, CPF, Email, ValorMensal, Ativo, DataAdesao, DataCriacao) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        "ITA CORRETORA MASTER", cpfMaster, "master@itau.com.br", 0, 1, DateTime.UtcNow, DateTime.UtcNow);
                    
                    clienteMaster = await context.Clientes.FirstOrDefaultAsync(c => c.CPF == cpfMaster);
                }

                contaMaster = new ContaGrafica(clienteMaster.Id, "99999-9", ContaTipo.MASTER);
                await context.ContasGraficas.AddAsync(contaMaster);
                await context.SaveChangesAsync();
                
                await LogAsync("Information", "Conta Master e Cliente Master criados com sucesso.");
            }
        }

        private async Task SeedCotacoesAsync()
        {
            var temCotacoes = await context.Cotacoes.AnyAsync();
            if (!temCotacoes)
            {
                logger.LogInformation("Tabela de cotações vazia. Iniciando importação...");
                
                string cotacoesPath = Path.Combine(AppContext.BaseDirectory, "cotacoes");

                if (!Directory.Exists(cotacoesPath))
                {
                    var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
                    while (currentDir != null && !Directory.Exists(Path.Combine(currentDir.FullName, "cotacoes")))
                        currentDir = currentDir.Parent;
                    
                    if (currentDir != null)
                        cotacoesPath = Path.Combine(currentDir.FullName, "cotacoes");
                }

                if (Directory.Exists(cotacoesPath))
                {
                    string[] arquivos = Directory.GetFiles(cotacoesPath, "*.TXT");
                    foreach (string arquivo in arquivos)
                    {
                        await cotacaoService.ProcessarArquivoAsync(arquivo);
                    }
                }
            }
        }

        private async Task LogAsync(string nivel, string mensagem, string? excecao = null)
        {
            try
            {
                var log = new Log(nivel, mensagem, excecao, nameof(InicializadorBanco));
                await logRepository.AddAsync(log);
                await logRepository.SaveChangesAsync();
            }
            catch
            {
                logger.LogError("Falha ao persistir log no banco: {Mensagem}", mensagem);
            }
        }
    }
}

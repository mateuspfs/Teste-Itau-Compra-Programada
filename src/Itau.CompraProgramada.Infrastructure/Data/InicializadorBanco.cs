using Microsoft.EntityFrameworkCore;
using Itau.CompraProgramada.Domain.Entities;
using Microsoft.Extensions.Logging;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Interfaces.Imports;

namespace Itau.CompraProgramada.Infrastructure.Data
{
    public interface IInicializadorBanco
    {
        Task InicializarAsync();
    }

    public class InicializadorBanco(
        ApplicationDbContext context,
        ICotacaoImport cotacaoService,
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

                var temCotacoes = await context.Cotacoes.AnyAsync();
                
                if (!temCotacoes)
                {
                    await LogAsync("Information", "Tabela de cotações vazia. Iniciando importação...");
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
                            logger.LogInformation("Processando arquivo: {Arquivo}", Path.GetFileName(arquivo));
                            await cotacaoService.ProcessarArquivoAsync(arquivo);
                        }
                    }
                    else
                    {
                        await LogAsync("Warning", "Pasta 'cotacoes' não encontrada para o Seed.");
                    }
                }

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

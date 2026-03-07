using Itau.CompraProgramada.Domain.Utils;
using Microsoft.EntityFrameworkCore;
using Itau.CompraProgramada.Domain.Entities;
using Microsoft.Extensions.Logging;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Processor;
using Microsoft.Extensions.Configuration;

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
        IConfiguration configuration,
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
                var cpfMaster = CpfUtils.GerarCpfRelativo(1);
                var clienteMaster = await context.Clientes.FirstOrDefaultAsync(c => c.CPF == cpfMaster);

                if (clienteMaster == null)
                {
                    clienteMaster = new Cliente("ITA CORRETORA MASTER", cpfMaster, "master@itau.com.br", 0);
                    await context.Clientes.AddAsync(clienteMaster);
                    await context.SaveChangesAsync();
                }

                contaMaster = new ContaGrafica(clienteMaster.Id, "99999-9", ContaTipo.MASTER);
                
                await context.ContasGraficas.AddAsync(contaMaster);
                await context.SaveChangesAsync();

                // RN-004 adaptada: Inicializar custódia zerada para a Master também
                var cestaAtiva = await context.CestasRecomendacao.FirstOrDefaultAsync(c => c.Ativa);
                if (cestaAtiva != null)
                {
                    logger.LogInformation("Inicializando custódia da Conta Master para a cesta ativa {cestaId}", cestaAtiva.Id);
                    var itensCesta = await context.ItensCesta.Where(i => i.CestaId == cestaAtiva.Id).ToListAsync();
                    foreach (var item in itensCesta)
                    {
                        var custodia = new Custodia(contaMaster.Id, item.Ticker, 0, 0);
                        await context.Custodias.AddAsync(custodia);
                    }
                    await context.SaveChangesAsync();
                }
                
                await LogAsync("Information", "Conta Master e Custódia Inicial criadas com sucesso.");
            }
        }

        private async Task SeedCotacoesAsync()
        {
            var temCotacoes = await context.Cotacoes.AnyAsync();
            if (!temCotacoes)
            {
                logger.LogInformation("Tabela de cotações vazia. Iniciando importação...");
                
                string cotacoesPath = configuration["SeedData:CotacoesPath"] ?? Path.Combine(AppContext.BaseDirectory, "cotacoes");

                if (!Directory.Exists(cotacoesPath))
                {
                    logger.LogWarning("Diretório de cotações não encontrado em {path}. Tentando subir níveis...", cotacoesPath);
                    var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
                    while (currentDir != null && !Directory.Exists(Path.Combine(currentDir.FullName, "cotacoes")) && !Directory.Exists(Path.Combine(currentDir.FullName, "Resources")))
                        currentDir = currentDir.Parent;
                    
                    if (currentDir != null)
                    {
                        var tryPath = Path.Combine(currentDir.FullName, "cotacoes");
                        if (!Directory.Exists(tryPath)) tryPath = Path.Combine(currentDir.FullName, "Resources");
                        cotacoesPath = tryPath;
                    }
                }

                if (Directory.Exists(cotacoesPath))
                {
                    logger.LogInformation("Importando cotações de {path}", cotacoesPath);
                    string[] arquivos = [.. Directory.GetFiles(cotacoesPath, "*.*").Where(f => f.EndsWith(".TXT", StringComparison.OrdinalIgnoreCase) 
                                        || f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))];
                        
                    foreach (string arquivo in arquivos)
                        await cotacaoService.ProcessarArquivoAsync(arquivo);
                }
                else
                {
                    logger.LogWarning("Diretório de cotações não pôde ser resolvido.");
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

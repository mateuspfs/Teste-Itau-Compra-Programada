using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Interfaces.Processor;

namespace Itau.CompraProgramada.Worker;

public class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private DateTime? _ultimaExecucao;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var agora = DateTime.Now;

            if (_ultimaExecucao?.Date != agora.Date)
            {
                logger.LogInformation("Iniciando processamento diário em: {time}", agora);
                
                try 
                {
                    await ProcessarDiaAsync(agora);
                    _ultimaExecucao = agora;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro no processamento diário.");
                }
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessarDiaAsync(DateTime data)
    {
        using var scope = scopeFactory.CreateScope();
        var cotacaoProcessor = scope.ServiceProvider.GetRequiredService<ICotacaoProcessor>();
        var motorCompra = scope.ServiceProvider.GetRequiredService<IMotorCompraEngine>();

        string cotacoesPath = LocalizarPastaCotacoes();
        
        // Formatar nome do arquivo esperado: COTAHIST_D<DDMMYYYY>.TXT
        string fileName = $"COTAHIST_D{data:ddMMyyyy}.TXT";
        string filePath = Path.Combine(cotacoesPath, fileName);

        if (File.Exists(filePath))
        {
            logger.LogInformation("Arquivo encontrado: {file}. Iniciando importação.", fileName);
            await cotacaoProcessor.ProcessarArquivoAsync(filePath);

            logger.LogInformation("Ativando Motor de Compra para o dia {data}", data.ToShortDateString());
            await motorCompra.ExecutarProcessamentoDiarioAsync(data);
        }
        else
        {
            logger.LogWarning("Arquivo {file} não encontrado na pasta {path}. O processamento diário não será executado hoje.", fileName, cotacoesPath);
        }
    }

    private static string LocalizarPastaCotacoes()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "cotacoes");

        if (!Directory.Exists(path))
        {
            var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
            while (currentDir != null && !Directory.Exists(Path.Combine(currentDir.FullName, "cotacoes")))
                currentDir = currentDir.Parent;
            
            if (currentDir != null)
                path = Path.Combine(currentDir.FullName, "cotacoes");
        }

        return path;
    }
}

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
        // Inicia o consumo da fila SQS em background
        // O serviço é resolvido manualmente por ser Singleton, assim como o Worker
        var sqsConsumer = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Services.SqsConsumerService>();
        _ = Task.Run(() => sqsConsumer.ConsumirFilaAsync(stoppingToken), stoppingToken);

        // Rotina de verificação diária para execução do Motor de Compra (desacoplado da ingestão de dados)
        while (!stoppingToken.IsCancellationRequested)
        {
            var agora = DateTime.Now;

            if (_ultimaExecucao?.Date != agora.Date)
            {
                logger.LogInformation("Verificando necessidade de rodar o Motor de Compra em: {time}", agora);
                
                try 
                {
                    await ProcessarDiaAsync(agora);
                    _ultimaExecucao = agora;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro na ativação do motor de compra.");
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

        /* 
        ===================================================================
        LÓGICA ANTIGA (LEITURA LOCAL) - SUBSTITUÍDA POR SQS CONSUMER
        ===================================================================
        string cotacoesPath = LocalizarPastaCotacoes();
        string fileName = $"COTAHIST_D{data:ddMMyyyy}.TXT";
        string filePath = Path.Combine(cotacoesPath, fileName);

        if (File.Exists(filePath))
        {
            logger.LogInformation("Arquivo encontrado: {file}. Iniciando importação.", fileName);
            await cotacaoProcessor.ProcessarArquivoAsync(filePath);
        }
        else
        {
            logger.LogWarning("Arquivo {file} não encontrado na pasta {path}.", fileName, cotacoesPath);
        }
        ===================================================================
        */

        // A execução do Motor de Compra agora ocorre de forma independente,
        // utilizando as cotações que já foram ingeridas assincronamente via SQS.
        logger.LogInformation("Ativando Motor de Compra para o dia {data}", data.ToShortDateString());
        await motorCompra.ExecutarProcessamentoDiarioAsync(data);
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

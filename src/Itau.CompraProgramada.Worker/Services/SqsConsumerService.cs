using Amazon.SQS;
using Amazon.SQS.Model;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using System.Text.Json;

namespace Itau.CompraProgramada.Worker.Services;

// DTO para mapeamento do JSON enviado pela Lambda Python
public class CotacaoSqsDto
{
    public DateTime Data { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal Abertura { get; set; }
    public decimal Fechamento { get; set; }
    public decimal Maximo { get; set; }
    public decimal Minimo { get; set; }
}

public class SqsConsumerService(
    ILogger<SqsConsumerService> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration)
{
    private readonly string _queueUrl = configuration["AWS:SqsQueueUrl"] ?? "https://sqs.us-east-2.amazonaws.com/123456789/b3-quotes-events-queue";
    // Instanciando o cliente SQS diretamente para o escopo do projeto (em produção o ideal é injetar via DI)
    private readonly AmazonSQSClient _sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.USEast2); 

    public async Task ConsumirFilaAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Iniciando consumo da Fila SQS: {queueUrl}", _queueUrl);

        var request = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1, // Consumimos 1 mensagem por vez (cada mensagem contém um lote de até 500 cotações)
            WaitTimeSeconds = 20
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                if (response.Messages.Count > 0)
                {
                    foreach (var message in response.Messages)
                    {
                        await ProcessarMensagemEmLoteAsync(message);
                        
                        // Após salvar no banco com sucesso, apagamos a mensagem da fila (ACK)
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro crítico ao consumir a fila SQS. Tentando novamente em 5 segundos...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessarMensagemEmLoteAsync(Message message)
    {
        // O body da mensagem é um Array JSON com 500 cotações
        var loteDtos = JsonSerializer.Deserialize<List<CotacaoSqsDto>>(message.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (loteDtos == null || loteDtos.Count == 0) return;

        logger.LogInformation("Lote recebido com {count} cotações da fila SQS. Preparando inserção no banco.", loteDtos.Count);

        var entidades = loteDtos.Select(dto => new Cotacao(
            dto.Data,
            dto.Ticker,
            dto.Abertura,
            dto.Fechamento,
            dto.Maximo,
            dto.Minimo
        )).ToList();

        // Abertura de um escopo manual no Worker para poder resolver os Repositories (que são Scoped)
        using var scope = scopeFactory.CreateScope();
        var cotacaoRepository = scope.ServiceProvider.GetRequiredService<ICotacaoRepository>();

        // Inserção em Lote (Bulk Insert) para otimizar a gravação no banco de dados
        await cotacaoRepository.AddRangeAsync(entidades);
        await cotacaoRepository.SaveChangesAsync();

        logger.LogInformation("Lote de {count} cotações persistido com sucesso no banco de dados!", entidades.Count);
    }
}

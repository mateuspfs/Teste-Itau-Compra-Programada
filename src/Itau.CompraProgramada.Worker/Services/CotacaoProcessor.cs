using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Processor;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using System.Globalization;

namespace Itau.CompraProgramada.Worker.Services
{
    public class CotacaoProcessor(
        ICotacaoRepository cotacaoRepository, 
        ILogRepository logRepository,
        ILogger<CotacaoProcessor> logger) : ICotacaoProcessor
    {
        public async Task<int> ProcessarArquivoAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    await LogAsync("Warning", $"Arquivo não encontrado: {filePath}");
                    return 0;
                }

                await LogAsync("Information", $"Iniciando processamento do arquivo: {Path.GetFileName(filePath)}");

                var cotacoesProcessadas = 0;
                const int batchSize = 5000;
                var batch = new List<Cotacao>(batchSize);

                using var reader = new StreamReader(filePath);
                string? linha;
                while ((linha = await reader.ReadLineAsync()) != null)
                {
                    if (linha.StartsWith("01"))
                    {
                        var cotacao = ParseLinha(linha);
                        if (cotacao != null)
                        {
                            batch.Add(cotacao);
                            cotacoesProcessadas++;

                            if (batch.Count >= batchSize)
                            {
                                await cotacaoRepository.AddRangeAsync(batch);
                                await cotacaoRepository.SaveChangesAsync();
                                batch.Clear();
                            }
                        }
                    }
                }

                if (batch.Count > 0)
                {
                    await cotacaoRepository.AddRangeAsync(batch);
                    await cotacaoRepository.SaveChangesAsync();
                }

                await LogAsync("Information", $"Processamento concluído: {cotacoesProcessadas} cotações importadas do arquivo {Path.GetFileName(filePath)}.");
                return cotacoesProcessadas;
            }
            catch (Exception ex)
            {
                await LogAsync("Error", $"Erro ao processar arquivo {Path.GetFileName(filePath)}", ex.ToString());
                logger.LogError(ex, "Erro ao processar arquivo {FilePath}", filePath);
                throw;
            }
        }

        private async Task LogAsync(string nivel, string mensagem, string? excecao = null)
        {
            var log = new Log(nivel, mensagem, excecao, nameof(CotacaoProcessor));
            await logRepository.AddAsync(log);
            await logRepository.SaveChangesAsync();
        }

        private static Cotacao? ParseLinha(string linha)
        {
            try
            {
                var dataStr = linha.Substring(2, 8);
                var data = DateTime.ParseExact(dataStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                var ticker = linha.Substring(12, 12).Trim();
                
                var abertura = ParseB3Decimal(linha.Substring(56, 13));
                var maximo = ParseB3Decimal(linha.Substring(69, 13));
                var minimo = ParseB3Decimal(linha.Substring(82, 13));
                var fechamento = ParseB3Decimal(linha.Substring(108, 13));

                return new Cotacao(data, ticker, abertura, fechamento, maximo, minimo);
            }
            catch
            {
                return null;
            }
        }

        private static decimal ParseB3Decimal(string value)
        {
            if (long.TryParse(value, out long rawValue)) return (decimal)rawValue / 100m;
            
            return 0;
        }
    }
}

using FluentAssertions;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Worker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Itau.CompraProgramada.Tests.Unit.Worker
{
    public class CotacaoProcessorTests : IDisposable
    {
        private readonly Mock<ICotacaoRepository> _repoMock;
        private readonly Mock<ILogRepository> _logMock;
        private readonly Mock<ILogger<CotacaoProcessor>> _loggerMock;
        private readonly CotacaoProcessor _processor;
        private readonly string _tempFile;

        public CotacaoProcessorTests()
        {
            _repoMock = new Mock<ICotacaoRepository>();
            _logMock = new Mock<ILogRepository>();
            _loggerMock = new Mock<ILogger<CotacaoProcessor>>();
            _processor = new CotacaoProcessor(_repoMock.Object, _logMock.Object, _loggerMock.Object);
            _tempFile = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile)) File.Delete(_tempFile);
        }

        [Fact]
        public async Task ProcessarArquivoAsync_ShouldParseCorrectly()
        {
            // Arrange
            // Sample line from B3 layout (simplified based on ParseLinha offsets)
            // 01 20260305 12 TICKER      56-Ab (13) 69-Max (13) 82-Min (13) 108-Fech (13)
            // Index 0-1: 01
            // Index 2-9: 20260305 (dataStr)
            // Index 12-23: PETR4 (ticker)
            // Index 56-68: 0000000003000 (abertura = 30.00)
            // Index 69-81: 0000000003500 (maximo = 35.00)
            // Index 82-94: 0000000002900 (minimo = 29.00)
            // Index 108-120: 0000000003400 (fechamento = 34.00)
            
            var line = "012026030512PETR4       " + new string(' ', 32) + 
                       "0000000003000" + "0000000003500" + "0000000002900" + 
                       new string(' ', 13) + "0000000003400";

            await File.WriteAllLinesAsync(_tempFile, new[] { "HEADER", line, "TRAILER" });

            // Act
            var result = await _processor.ProcessarArquivoAsync(_tempFile);

            // Assert
            result.Should().Be(1);
            _repoMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Cotacao>>(it => it.First().Ticker == "PETR4")), Times.Once);
        }
    }
}

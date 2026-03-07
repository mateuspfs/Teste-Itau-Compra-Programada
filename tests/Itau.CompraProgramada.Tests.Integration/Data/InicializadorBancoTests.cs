using FluentAssertions;
using Itau.CompraProgramada.Domain.Interfaces.Processor;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Itau.CompraProgramada.Tests.Integration.Data
{
    [Collection("Database collection")]
    public class InicializadorBancoTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
    {
        [Fact]
        public async Task InicializarAsync_ShouldSeedMasterData()
        {
            // Arrange
            var cotacaoProcessorMock = new Mock<ICotacaoProcessor>();
            var logRepoMock = new Mock<ILogRepository>();
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<InicializadorBanco>>();

            var inicializador = new InicializadorBanco(
                Context,
                cotacaoProcessorMock.Object,
                logRepoMock.Object,
                configMock.Object,
                loggerMock.Object);

            // Act
            await inicializador.InicializarAsync();

            // Assert
            var masterClient = await Context.Clientes.FirstOrDefaultAsync(c => c.Nome == "ITA CORRETORA MASTER");
            masterClient.Should().NotBeNull();
            
            var masterAccount = await Context.ContasGraficas.FirstOrDefaultAsync(c => c.ClienteId == masterClient!.Id);
            masterAccount.Should().NotBeNull();
            masterAccount!.NumeroConta.Should().Be("99999-9");
        }
    }
}

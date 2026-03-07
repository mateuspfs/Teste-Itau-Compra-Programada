using FluentAssertions;
using Itau.CompraProgramada.Domain.Entities;

namespace Itau.CompraProgramada.Tests.Unit.Domain.Entities
{
    public class HistoricoValorMensalTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            long clienteId = 123;
            decimal valorAnterior = 1000m;
            decimal valorNovo = 1500m;

            // Act
            var historico = new HistoricoValorMensal(clienteId, valorAnterior, valorNovo);

            // Assert
            historico.ClienteId.Should().Be(clienteId);
            historico.ValorAnterior.Should().Be(valorAnterior);
            historico.ValorNovo.Should().Be(valorNovo);
            historico.DataAlteracao.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }
    }
}

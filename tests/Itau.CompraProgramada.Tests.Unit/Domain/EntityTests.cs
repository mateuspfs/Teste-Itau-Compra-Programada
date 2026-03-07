using FluentAssertions;
using Itau.CompraProgramada.Domain.Entities;

namespace Itau.CompraProgramada.Tests.Unit.Domain
{
    public class CustodiaTests
    {
        [Fact]
        public void AtualizarPosicao_ShouldUpdateQuantityAndPrice()
        {
            // Arrange
            var custodia = new Custodia(1, "PETR4", 10, 30.00m);

            // Act
            custodia.AtualizarPosicao(20, 35.00m);

            // Assert
            custodia.Quantidade.Should().Be(20);
            custodia.PrecoMedio.Should().Be(35.00m);
        }
    }

    public class ClienteTests
    {
        [Fact]
        public void Desativar_ShouldSetAtivoToFalse()
        {
            // Arrange
            var cliente = new Cliente("João", "52998224725", "joao@email.com", 1000m);

            // Act
            cliente.Desativar();

            // Assert
            cliente.Ativo.Should().BeFalse();
        }

        [Fact]
        public void AlterarValorMensal_ShouldUpdateValue()
        {
            // Arrange
            var cliente = new Cliente("João", "52998224725", "joao@email.com", 1000m);

            // Act
            cliente.AlterarValorMensal(2000m);

            // Assert
            cliente.ValorMensal.Should().Be(2000m);
        }
    }
}

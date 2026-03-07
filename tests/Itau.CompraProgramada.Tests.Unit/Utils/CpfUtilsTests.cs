using FluentAssertions;
using Itau.CompraProgramada.Domain.Utils;
using Xunit;

namespace Itau.CompraProgramada.Tests.Unit.Utils
{
    public class CpfUtilsTests
    {
        [Theory]
        [InlineData("12345678909", true)]
        [InlineData("11111111111", false)] // Digitos iguais
        [InlineData("123", false)]         // Tamanho errado
        [InlineData("", false)]            // Vazio
        [InlineData(null, false)]          // Nulo
        [InlineData("12345678900", false)] // Digito verificador errado
        public void Validar_DeveRetornarEsperado(string cpf, bool esperado)
        {
            // Act
            var resultado = CpfUtils.Validar(cpf);

            // Assert
            resultado.Should().Be(esperado);
        }

        [Fact]
        public void GerarCpfRelativo_DeveGerarCpfValido()
        {
            // Arrange
            int semente = 123456789;

            // Act
            var cpf = CpfUtils.GerarCpfRelativo(semente);

            // Assert
            CpfUtils.Validar(cpf).Should().BeTrue();
        }

        [Fact]
        public void GerarCpfRelativo_ComSementeBaixa_DeveGerarCpfValido()
        {
            // Arrange
            int semente = 1;

            // Act
            var cpf = CpfUtils.GerarCpfRelativo(semente);

            // Assert
            cpf.Length.Should().Be(11);
            CpfUtils.Validar(cpf).Should().BeTrue();
        }
    }
}

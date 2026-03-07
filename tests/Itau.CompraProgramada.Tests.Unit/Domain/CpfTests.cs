using FluentAssertions;
using Itau.CompraProgramada.Domain.ValueObjects;

namespace Itau.CompraProgramada.Tests.Unit.Domain
{
    public class CpfTests
    {
        [Theory]
        [InlineData("529.982.247-25", "52998224725")]
        [InlineData("52998224725", "52998224725")]
        [InlineData(" 529.982.247-25 ", "52998224725")]
        public void Constructor_ShouldCleanCpf(string input, string expected)
        {
            var cpf = new Cpf(input);
            cpf.Valor.Should().Be(expected);
        }

        [Theory]
        [InlineData("111.111.111-11")]
        [InlineData("123456789")]
        [InlineData("abc45678900")]
        public void Validar_ShouldReturnFalse_ForInvalidCpf(string input)
        {
            Cpf.Validar(input).Should().BeFalse();
        }

        [Theory]
        [InlineData("52998224725")] // Valid sample
        [InlineData("39491150804")] // Valid sample
        public void Validar_ShouldReturnTrue_ForValidCpf(string input)
        {
            Cpf.Validar(input).Should().BeTrue();
        }
    }
}

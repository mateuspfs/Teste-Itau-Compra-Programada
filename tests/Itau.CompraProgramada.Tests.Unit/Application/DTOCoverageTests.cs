using FluentAssertions;
using Itau.CompraProgramada.Application.DTOs.Admin;
using Itau.CompraProgramada.Application.DTOs.Clientes;

namespace Itau.CompraProgramada.Tests.Unit.Application
{
    public class DTOCoverageTests
    {
        [Fact]
        public void CestaResumoDTO_ShouldExerciseProperties()
        {
            // Arrange & Act
            var dto = new CestaResumoDTO
            {
                CestaId = 1,
                Nome = "Teste",
                DataDesativacao = DateTime.UtcNow
            };

            // Assert
            dto.CestaId.Should().Be(1);
            dto.Nome.Should().Be("Teste");
            dto.DataDesativacao.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void AlterarValorMensalRequest_ShouldExerciseProperties()
        {
            // Arrange & Act
            var dto = new AlterarValorMensalRequest { NovoValorMensal = 500m };

            // Assert
            dto.NovoValorMensal.Should().Be(500m);
        }

        [Fact]
        public void AlterarValorMensalResponse_ShouldExerciseProperties()
        {
            // Arrange & Act
            var now = DateTime.UtcNow;
            var dto = new AlterarValorMensalResponse
            {
                ClienteId = 1,
                ValorMensalAnterior = 300m,
                ValorMensalNovo = 500m,
                DataAlteracao = now,
                Mensagem = "Ok"
            };

            // Assert
            dto.ClienteId.Should().Be(1);
            dto.ValorMensalAnterior.Should().Be(300m);
            dto.ValorMensalNovo.Should().Be(500m);
            dto.DataAlteracao.Should().Be(now);
            dto.Mensagem.Should().Be("Ok");
        }

        [Fact]
        public void SaidaProdutoResponse_ShouldExerciseProperties()
        {
            // Arrange & Act
            var now = DateTime.UtcNow;
            var dto = new SaidaProdutoResponse
            {
                ClienteId = 1,
                Nome = "Teste",
                Ativo = false,
                DataSaida = now,
                Mensagem = "Saida"
            };

            // Assert
            dto.ClienteId.Should().Be(1);
            dto.Nome.Should().Be("Teste");
            dto.Ativo.Should().BeFalse();
            dto.DataSaida.Should().Be(now);
            dto.Mensagem.Should().Be("Saida");
        }
    }
}

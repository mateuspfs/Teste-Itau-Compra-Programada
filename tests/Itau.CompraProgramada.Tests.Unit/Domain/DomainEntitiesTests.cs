using FluentAssertions;
using Xunit;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Tests.Unit.Helpers;

namespace Itau.CompraProgramada.Tests.Unit.Domain
{
    public class DomainEntitiesTests
    {
        [Fact]
        public void Cliente_DeveIniciarComoAtivoEComDataAdesao()
        {
            // Act
            var cliente = new Cliente("Teste", "12345678909", "teste@teste.com", 1000).SetId(1);

            // Assert
            cliente.Ativo.Should().BeTrue();
            cliente.DataAdesao.Should().NotBe(default(DateTime));
            cliente.DataCriacao.Date.Should().Be(DateTime.UtcNow.Date);
        }

        [Fact]
        public void Custodia_DeveAtualizarPosicaoCorretamente()
        {
            // Arrange
            var custodia = new Custodia(1, "ITUB4", 10, 30.0m);

            // Act
            custodia.AtualizarPosicao(15, 32.0m);

            // Assert
            custodia.Quantidade.Should().Be(15);
            custodia.PrecoMedio.Should().Be(32.0m);
        }

        [Fact]
        public void CestaRecomendacao_DeveSerCriadaCorretamente()
        {
            // Act
            var cesta = new CestaRecomendacao("Cesta Agressiva");

            // Assert
            cesta.Nome.Should().Be("Cesta Agressiva");
            cesta.Ativa.Should().BeTrue();
        }

        [Fact]
        public void ContaGrafica_DeveSerCriadaComTipoCorreto()
        {
            // Act
            var conta = new ContaGrafica(1, "12345-6", ContaTipo.MASTER);

            // Assert
            conta.ClienteId.Should().Be(1);
            conta.NumeroConta.Should().Be("12345-6");
            conta.Tipo.Should().Be(ContaTipo.MASTER);
        }

        [Fact]
        public void OrdemCompra_DeveSerCriadaCorretamente()
        {
            // Act
            var ordem = new OrdemCompra(1, "ITUB4", 10, 30.0m, TipoMercado.LOTE);

            // Assert
            ordem.Ticker.Should().Be("ITUB4");
            ordem.Quantidade.Should().Be(10);
            ordem.PrecoUnitario.Should().Be(30.0m);
            ordem.TipoMercado.Should().Be(TipoMercado.LOTE);
        }
    }
}

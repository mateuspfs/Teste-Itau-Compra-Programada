using Moq;
using FluentAssertions;
using Xunit;
using Itau.CompraProgramada.Application.Services;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Application.Exceptions;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Tests.Unit.Helpers;

namespace Itau.CompraProgramada.Tests.Unit.Application
{
    public class CarteiraServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly Mock<IContaGraficaRepository> _contaRepoMock;
        private readonly Mock<ICustodiaRepository> _custodiaRepoMock;
        private readonly Mock<ICotacaoRepository> _cotacaoRepoMock;
        private readonly CarteiraService _service;

        public CarteiraServiceTests()
        {
            _clienteRepoMock = new Mock<IClienteRepository>();
            _contaRepoMock = new Mock<IContaGraficaRepository>();
            _custodiaRepoMock = new Mock<ICustodiaRepository>();
            _cotacaoRepoMock = new Mock<ICotacaoRepository>();
            _service = new CarteiraService(
                _clienteRepoMock.Object,
                _contaRepoMock.Object,
                _custodiaRepoMock.Object,
                _cotacaoRepoMock.Object);
        }

        [Fact]
        public async Task ObterCarteira_DeveLancarNotFound_QuandoClienteNaoExiste()
        {
            // Arrange
            _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((Cliente)null);

            // Act
            var result = await _service.ObterCarteiraPorClienteAsync(1);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ObterCarteira_DeveRetornarNotFound_QuandoContaFilhoteNaoExiste()
        {
            // Arrange
            var cliente = new Cliente("Joao", "12345678909", "joao@teste.com", 1000).SetId(1);
            _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cliente);
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica>());

            // Act
            var result = await _service.ObterCarteiraPorClienteAsync(1);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ObterCarteira_DeveRetornarSucesso_QuandoDadosSaoValidos()
        {
            // Arrange
            var clienteId = 1L;
            var contaId = 10L;
            var cliente = new Cliente("Joao", "12345678909", "joao@teste.com", 1000).SetId(clienteId);
            var conta = new ContaGrafica(clienteId, "12345-6", ContaTipo.FILHOTE).SetId(contaId);
            
            var custodias = new List<Custodia>
            {
                new Custodia(contaId, "ITUB4", 10, 30.0m) // Valor Investido = 300
            };

            var cotacao = new Cotacao(DateTime.Now, "ITUB4", 34.0m, 35.0m, 36.0m, 33.0m); // Valor Atual = 350, PL = 50

            _clienteRepoMock.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { conta });
            _custodiaRepoMock.Setup(r => r.GetByContaGraficaIdAsync(contaId)).ReturnsAsync(custodias);
            _cotacaoRepoMock.Setup(r => r.GetUltimaCotacaoAsync("ITUB4")).ReturnsAsync(cotacao);

            // Act
            var result = await _service.ObterCarteiraPorClienteAsync(clienteId);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data.Resumo.ValorTotalInvestido.Should().Be(300);
            result.Data.Resumo.ValorAtualCarteira.Should().Be(350);
            result.Data.Resumo.PLTotal.Should().Be(50);
            result.Data.Resumo.RentabilidadePercentual.Should().BeApproximately(16.66m, 0.01m);
            result.Data.Ativos.Should().HaveCount(1);
            result.Data.Ativos[0].Ticker.Should().Be("ITUB4");
            result.Data.Ativos[0].ComposicaoCarteira.Should().Be(100);
        }
    }
}

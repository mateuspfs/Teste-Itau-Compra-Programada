using FluentAssertions;
using Itau.CompraProgramada.Application.DTOs.Admin;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Application.Services;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Moq;
using System.Linq.Expressions;
using Itau.CompraProgramada.Tests.Unit.Helpers;

namespace Itau.CompraProgramada.Tests.Unit.Application
{
    public class AdminServiceTests
    {
        private readonly Mock<ICestaRecomendacaoRepository> _cestaRepoMock;
        private readonly Mock<IItemCestaRepository> _itemRepoMock;
        private readonly Mock<IRebalanceamentoService> _rebalServiceMock;
        private readonly Mock<IContaGraficaRepository> _contaRepoMock;
        private readonly Mock<ICustodiaRepository> _custodiaRepoMock;
        private readonly Mock<ICotacaoRepository> _cotacaoRepoMock;
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            _cestaRepoMock = new Mock<ICestaRecomendacaoRepository>();
            _itemRepoMock = new Mock<IItemCestaRepository>();
            _rebalServiceMock = new Mock<IRebalanceamentoService>();
            _contaRepoMock = new Mock<IContaGraficaRepository>();
            _custodiaRepoMock = new Mock<ICustodiaRepository>();
            _cotacaoRepoMock = new Mock<ICotacaoRepository>();

            _service = new AdminService(
                _cestaRepoMock.Object,
                _itemRepoMock.Object,
                _rebalServiceMock.Object,
                _contaRepoMock.Object,
                _custodiaRepoMock.Object,
                _cotacaoRepoMock.Object);
        }

        [Fact]
        public async Task CadastrarAlterarCestaAsync_ShouldFail_IfNotExactly5Ativos()
        {
            // Arrange
            var request = new CestaRequest 
            { 
                Nome = "Cesta Errada", 
                Itens = new List<ItemCestaDTO> { new ItemCestaDTO { Ticker = "PETR4", Percentual = 100 } } 
            };

            // Act
            var result = await _service.CadastrarAlterarCestaAsync(request);
 
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("deve conter exatamente 5 ativos");
        }

        [Fact]
        public async Task CadastrarAlterarCestaAsync_ShouldSucceed_WithValidData()
        {
            // Arrange
            var request = new CestaRequest 
            { 
                Nome = "Cesta Certa", 
                Itens = new List<ItemCestaDTO> 
                { 
                    new() { Ticker = "PETR4", Percentual = 20 },
                    new() { Ticker = "VALE3", Percentual = 20 },
                    new() { Ticker = "ITUB4", Percentual = 20 },
                    new() { Ticker = "BBDC4", Percentual = 20 },
                    new() { Ticker = "ABEV3", Percentual = 20 }
                } 
            };

            _cestaRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()))
                          .ReturnsAsync((CestaRecomendacao)null);

            // Act
            var response = await _service.CadastrarAlterarCestaAsync(request);

            // Assert
            response.Data.Nome.Should().Be("Cesta Certa");
            _cestaRepoMock.Verify(x => x.AddAsync(It.IsAny<CestaRecomendacao>()), Times.Once);
            _itemRepoMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<ItemCesta>>(i => i.Count() == 5)), Times.Once);
        }

        [Fact]
        public async Task CadastrarAlterarCestaAsync_ShouldFail_IfSomaPercentualNot100()
        {
            // Arrange
            var request = new CestaRequest 
            { 
                Nome = "Cesta Errada", 
                Itens = new List<ItemCestaDTO> 
                { 
                    new() { Ticker = "P1", Percentual = 10 },
                    new() { Ticker = "P2", Percentual = 10 },
                    new() { Ticker = "P3", Percentual = 10 },
                    new() { Ticker = "P4", Percentual = 10 },
                    new() { Ticker = "P5", Percentual = 10 }
                } 
            };

            // Act
            var result = await _service.CadastrarAlterarCestaAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("100%");
        }

        [Fact]
        public async Task CadastrarAlterarCestaAsync_ShouldFail_IfPercentualIsNegative()
        {
            // Arrange
            var request = new CestaRequest 
            { 
                Nome = "Cesta Errada", 
                Itens = new List<ItemCestaDTO> 
                { 
                    new() { Ticker = "P1", Percentual = 110 },
                    new() { Ticker = "P2", Percentual = -10 },
                    new() { Ticker = "P3", Percentual = 0 },
                    new() { Ticker = "P4", Percentual = 0 },
                    new() { Ticker = "P5", Percentual = 0 }
                } 
            };

            // Act
            var result = await _service.CadastrarAlterarCestaAsync(request);
 
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("maior que 0%");
        }

        [Fact]
        public async Task CadastrarAlterarCestaAsync_ShouldTriggerRebalance_IfBasketExists()
        {
            // Arrange
            var request = new CestaRequest 
            { 
                Nome = "Nova Cesta", 
                Itens = new List<ItemCestaDTO> 
                { 
                    new() { Ticker = "P1", Percentual = 20 },
                    new() { Ticker = "P2", Percentual = 20 },
                    new() { Ticker = "P3", Percentual = 20 },
                    new() { Ticker = "P4", Percentual = 20 },
                    new() { Ticker = "P5", Percentual = 20 }
                } 
            };

            var cestaExistente = new CestaRecomendacao("Antiga").SetId(10);
            _cestaRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()))
                          .ReturnsAsync(cestaExistente);
            _itemRepoMock.Setup(x => x.GetByCestaIdAsync(10)).ReturnsAsync(new List<ItemCesta>());

            // Act
            var response = await _service.CadastrarAlterarCestaAsync(request);

            // Assert
            response.Data.RebalanceamentoDisparado.Should().BeTrue();
            _rebalServiceMock.Verify(x => x.ProcessarRebalanceamentoPorMudancaCestaAsync(10, It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public async Task ObterCestaAtualAsync_ShouldThrowNotFound_IfNoActiveBasket()
        {
            // Arrange
            _cestaRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()))
                          .ReturnsAsync((CestaRecomendacao)null);

            // Act
            var result = await _service.ObterCestaAtualAsync();
 
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ObterCustodiaMasterAsync_ShouldThrowNotFound_IfMasterAccountMissing()
        {
            // Arrange
            _contaRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ContaGrafica>());

            // Act
            var result = await _service.ObterCustodiaMasterAsync();
 
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ObterCustodiaMasterAsync_ShouldCalculateResiduos_Correctly()
        {
            // Arrange
            var contaMaster = new ContaGrafica(1, "MST-001", Itau.CompraProgramada.Domain.Enums.ContaTipo.MASTER).SetId(100);
            _contaRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { contaMaster });

            var custodias = new List<Custodia> 
            { 
                new Custodia(100, "PETR4", 5, 30m),
                new Custodia(100, "VALE3", 0, 60m) // Quantidade zerada deve ser ignorada
            };
            _custodiaRepoMock.Setup(x => x.GetByContaGraficaIdAsync(100)).ReturnsAsync(custodias);

            _cotacaoRepoMock.Setup(x => x.GetUltimaCotacaoAsync("PETR4"))
                            .ReturnsAsync(new Cotacao(DateTime.UtcNow, "PETR4", 32m, 32m, 32m, 32m));

            // Act
            var response = await _service.ObterCustodiaMasterAsync();

            // Assert
            response.Data.Custodia.Should().HaveCount(1);
            response.Data.Custodia[0].Ticker.Should().Be("PETR4");
            response.Data.ValorTotalResiduo.Should().Be(160m); // 5 * 32
        }
    }
}

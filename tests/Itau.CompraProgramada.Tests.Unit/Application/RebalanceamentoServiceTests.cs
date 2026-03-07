using Moq;
using FluentAssertions;
using Xunit;
using Itau.CompraProgramada.Application.Services;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Tests.Unit.Helpers;
using Itau.CompraProgramada.Domain.Utils;

namespace Itau.CompraProgramada.Tests.Unit.Application
{
    public class RebalanceamentoServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly Mock<IContaGraficaRepository> _contaRepoMock;
        private readonly Mock<ICustodiaRepository> _custodiaRepoMock;
        private readonly Mock<IItemCestaRepository> _itemCestaRepoMock;
        private readonly Mock<ICotacaoRepository> _cotacaoRepoMock;
        private readonly Mock<IRebalanceamentoRepository> _rebalRepoMock;
        private readonly Mock<IIRService> _irServiceMock;
        private readonly RebalanceamentoService _service;

        public RebalanceamentoServiceTests()
        {
            _clienteRepoMock = new Mock<IClienteRepository>();
            _contaRepoMock = new Mock<IContaGraficaRepository>();
            _custodiaRepoMock = new Mock<ICustodiaRepository>();
            _itemCestaRepoMock = new Mock<IItemCestaRepository>();
            _cotacaoRepoMock = new Mock<ICotacaoRepository>();
            _rebalRepoMock = new Mock<IRebalanceamentoRepository>();
            _irServiceMock = new Mock<IIRService>();

            _service = new RebalanceamentoService(
                _clienteRepoMock.Object,
                _contaRepoMock.Object,
                _custodiaRepoMock.Object,
                _itemCestaRepoMock.Object,
                _cotacaoRepoMock.Object,
                _rebalRepoMock.Object,
                _irServiceMock.Object);
        }

        [Fact]
        public async Task ProcessarRebalanceamento_DeveRetornar_QuandoContaNaoExiste()
        {
            // Arrange
            var cliente = new Cliente("Joao", CpfUtils.GerarCpfRelativo(100), "joao@teste.com", 1000).SetId(1);
            _clienteRepoMock.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new List<Cliente> { cliente });
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica>()); // Sem contas

            // Act
            await _service.ProcessarRebalanceamentoPorMudancaCestaAsync(1, 2);

            // Assert
            _custodiaRepoMock.Verify(r => r.GetByContaGraficaIdAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarRebalanceamento_DeveRetornar_QuandoValorTotalCarteiraZero()
        {
            // Arrange
            var cliente = new Cliente("Joao", CpfUtils.GerarCpfRelativo(100), "joao@teste.com", 1000).SetId(1);
            var conta = new ContaGrafica(1, "12345-6", ContaTipo.FILHOTE).SetId(10);
            _clienteRepoMock.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new List<Cliente> { cliente });
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { conta });
            _custodiaRepoMock.Setup(r => r.GetByContaGraficaIdAsync(10)).ReturnsAsync(new List<Custodia>()); // Sem custódia

            // Act
            await _service.ProcessarRebalanceamentoPorMudancaCestaAsync(1, 2);

            // Assert
            _itemCestaRepoMock.Verify(r => r.GetByCestaIdAsync(It.IsAny<long>()), Times.AtLeastOnce);
            _rebalRepoMock.Verify(r => r.AddAsync(It.IsAny<Rebalanceamento>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarRebalanceamento_DeveExecutarLógicaDeVendaECustodia_QuandoCestaMuda()
        {
            // Arrange
            var cliente = new Cliente("Joao", CpfUtils.GerarCpfRelativo(100), "joao@teste.com", 1000).SetId(1);
            var conta = new ContaGrafica(1, "12345-6", ContaTipo.FILHOTE).SetId(10);

            // Custódia atual: ITUB4 (ticker que vai sair)
            var custodiaAntiga = new Custodia(10, "ITUB4", 100, 30.0m);
            var itemNovo = new ItemCesta(2, "PETR4", 100.0m); // Nova cesta 100% PETR4

            _clienteRepoMock.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new List<Cliente> { cliente });
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { conta });
            _custodiaRepoMock.Setup(r => r.GetByContaGraficaIdAsync(10)).ReturnsAsync(new List<Custodia> { custodiaAntiga });
            _itemCestaRepoMock.Setup(r => r.GetByCestaIdAsync(2)).ReturnsAsync(new List<ItemCesta> { itemNovo });

            // Cotações
            _cotacaoRepoMock.Setup(r => r.GetUltimaCotacaoAsync("ITUB4")).ReturnsAsync(new Cotacao(DateTime.Now, "ITUB4", 34.0m, 35.0m, 36.0m, 33.0m));
            _cotacaoRepoMock.Setup(r => r.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync(new Cotacao(DateTime.Now, "PETR4", 24.0m, 25.0m, 26.0m, 23.0m));

            // Act
            await _service.ProcessarRebalanceamentoPorMudancaCestaAsync(1, 2);

            // Assert
            // 1. Deve remover ITUB4
            _custodiaRepoMock.Verify(r => r.Remove(custodiaAntiga), Times.Once);

            // 2. Deve adicionar PETR4
            // Valor total carteira = 100 * 35 = 3500
            // Qtd PETR4 alvo = 3500 / 25 = 140
            _custodiaRepoMock.Verify(r => r.AddAsync(It.Is<Custodia>(c => c.Ticker == "PETR4" && c.Quantidade == 140)), Times.Once);

            // 3. Deve registrar rebalanceamentos no banco
            _rebalRepoMock.Verify(r => r.AddAsync(It.IsAny<Rebalanceamento>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task ProcessarRebalanceamento_DeveContinuar_QuandoPrecoCotacaoInvalido()
        {
            // Arrange
            var cliente = new Cliente("Joao", CpfUtils.GerarCpfRelativo(100), "joao@teste.com", 1000).SetId(1);
            var conta = new ContaGrafica(1, "12345-6", ContaTipo.FILHOTE).SetId(10);
            var custodia = new Custodia(10, "ITUB4", 100, 30.0m);
            var itemNovo = new ItemCesta(2, "PETR4", 100.0m);

            _clienteRepoMock.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new List<Cliente> { cliente });
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { conta });
            _custodiaRepoMock.Setup(r => r.GetByContaGraficaIdAsync(10)).ReturnsAsync(new List<Custodia> { custodia });
            _itemCestaRepoMock.Setup(r => r.GetByCestaIdAsync(2)).ReturnsAsync(new List<ItemCesta> { itemNovo });
            
            // Cotação inválida para PETR4
            _cotacaoRepoMock.Setup(r => r.GetUltimaCotacaoAsync("ITUB4")).ReturnsAsync(new Cotacao(DateTime.Now, "ITUB4", 30m, 30m, 30m, 30m));
            _cotacaoRepoMock.Setup(r => r.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync((Cotacao)null);

            // Act
            await _service.ProcessarRebalanceamentoPorMudancaCestaAsync(1, 2);

            // Assert
            _custodiaRepoMock.Verify(r => r.AddAsync(It.IsAny<Custodia>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarRebalanceamento_DeveAjustarPosicao_QuandoDiffQtdDiferenteDeZero()
        {
            // Arrange
            var cliente = new Cliente("Joao", CpfUtils.GerarCpfRelativo(100), "joao@teste.com", 1000).SetId(1);
            var conta = new ContaGrafica(1, "12345-6", ContaTipo.FILHOTE).SetId(10);
            
            // Custódia atual: ITUB4 (fica na cesta mas muda percentual)
            var custodia = new Custodia(10, "ITUB4", 100, 30.0m);
            var itemNovo = new ItemCesta(2, "ITUB4", 50.0m); // Agora só 50% da carteira

            _clienteRepoMock.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new List<Cliente> { cliente });
            _contaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { conta });
            _custodiaRepoMock.Setup(r => r.GetByContaGraficaIdAsync(10)).ReturnsAsync(new List<Custodia> { custodia });
            _itemCestaRepoMock.Setup(r => r.GetByCestaIdAsync(2)).ReturnsAsync(new List<ItemCesta> { itemNovo });
            
            _cotacaoRepoMock.Setup(r => r.GetUltimaCotacaoAsync("ITUB4")).ReturnsAsync(new Cotacao(DateTime.Now, "ITUB4", 30m, 30m, 30m, 30m));

            // Act
            await _service.ProcessarRebalanceamentoPorMudancaCestaAsync(1, 2);

            // Assert
            // Valor total = 100 * 30 = 3000. Alvo 50% = 1500. Preço 30 => Qtd Alvo 50.
            // Tinha 100, deve vender 50.
            custodia.Quantidade.Should().Be(50);
            _rebalRepoMock.Verify(r => r.AddAsync(It.Is<Rebalanceamento>(re => re.TickerVendido == "ITUB4" && re.ValorVenda == 1500)), Times.Once);
        }
    }
}

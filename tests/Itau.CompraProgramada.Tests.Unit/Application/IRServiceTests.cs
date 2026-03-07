using Moq;
using FluentAssertions;
using Xunit;
using Itau.CompraProgramada.Application.Services;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Messaging;
using Itau.CompraProgramada.Tests.Unit.Helpers;

namespace Itau.CompraProgramada.Tests.Unit.Application
{
    public class IRServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly Mock<IEventoIRRepository> _eventoIRRepoMock;
        private readonly Mock<IMessagingService> _messagingMock;
        private readonly Mock<IRebalanceamentoRepository> _rebalRepoMock;
        private readonly IRService _service;

        public IRServiceTests()
        {
            _clienteRepoMock = new Mock<IClienteRepository>();
            _eventoIRRepoMock = new Mock<IEventoIRRepository>();
            _messagingMock = new Mock<IMessagingService>();
            _rebalRepoMock = new Mock<IRebalanceamentoRepository>();
            _service = new IRService(
                _clienteRepoMock.Object,
                _eventoIRRepoMock.Object,
                _messagingMock.Object,
                _rebalRepoMock.Object);
        }

        [Fact]
        public async Task ProcessarIRDedoDuro_DeveCalcularValorMinimo_QuandoValorOperacaoEhBaixo()
        {
            // Arrange
            var cliente = new Cliente("Joao", "12345678909", "joao@teste.com", 1000).SetId(1);
            _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cliente);

            // Act
            // 10.00 * 0.00005 = 0.0005 (deve arredondar para 0.01 mínimo)
            await _service.ProcessarIRDedoDuroAsync(1, "ITUB4", 10.00m, "COMPRA", 1, 10.00m);

            // Assert
            _eventoIRRepoMock.Verify(r => r.AddAsync(It.Is<EventoIR>(e => e.ValorIR == 0.01m)), Times.Once);
            _messagingMock.Verify(m => m.PublishAsync("ir-events", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ProcessarIRDedoDuro_DeveCalcularValorCorreto_QuandoValorOperacaoEhAlto()
        {
            // Arrange
            var cliente = new Cliente("Joao", "12345678909", "joao@teste.com", 1000).SetId(1);
            _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cliente);

            // Act
            // 100000.00 * 0.00005 = 5.00
            await _service.ProcessarIRDedoDuroAsync(1, "ITUB4", 100000.00m, "VENDA", 1000, 100.00m);

            // Assert
            _eventoIRRepoMock.Verify(r => r.AddAsync(It.Is<EventoIR>(e => e.ValorIR == 5.00m)), Times.Once);
        }

        [Fact]
        public async Task ProcessarIRVendaMensal_NaoDeveProcessar_QuandoTotalVendasAbaixoDoLimite()
        {
            // Arrange
            var mes = DateTime.UtcNow.Month;
            var ano = DateTime.UtcNow.Year;
            var cliente = new Cliente("Joao", "12345678909", "joao@teste.com", 1000).SetId(1);
            
            var vendas = new List<Rebalanceamento>
            {
                new Rebalanceamento(1, RebalanceamentoTipo.MUDANCA_CESTA, "ITUB4", null, 15000.00m)
            };

            _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cliente);
            _rebalRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(vendas);

            // Act
            await _service.ProcessarIRVendaMensalAsync(1, mes, ano);

            // Assert
            _messagingMock.Verify(m => m.PublishAsync("ir-events", It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarIRVendaMensal_DeveProcessar_QuandoTotalVendasAcimaDoLimite()
        {
            // Arrange
            var mes = DateTime.UtcNow.Month;
            var ano = DateTime.UtcNow.Year;
            var cliente = new Cliente("Joao", "12345678909", "joao@teste.com", 1000).SetId(1);
            
            var vendas = new List<Rebalanceamento>
            {
                new Rebalanceamento(1, RebalanceamentoTipo.MUDANCA_CESTA, "ITUB4", null, 30000.00m, 5000.00m)
            };

            _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cliente);
            _rebalRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(vendas);

            // Act
            await _service.ProcessarIRVendaMensalAsync(1, mes, ano);

            // Assert
            // Total Vendas = 30k
            // Lucro Real = 5k
            // IR (20%) = 1000.00
            _messagingMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}

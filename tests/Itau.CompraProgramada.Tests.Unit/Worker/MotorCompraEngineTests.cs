using FluentAssertions;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Generic;
using Itau.CompraProgramada.Worker.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Itau.CompraProgramada.Tests.Unit.Helpers;
using Itau.CompraProgramada.Domain.Utils;

namespace Itau.CompraProgramada.Tests.Unit.Worker
{
    public class MotorCompraEngineTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IIRService> _irServiceMock;
        private readonly Mock<ILogger<MotorCompraEngine>> _loggerMock;
        private readonly MotorCompraEngine _engine;

        public MotorCompraEngineTests()
        {
            _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };
            _irServiceMock = new Mock<IIRService>();
            _loggerMock = new Mock<ILogger<MotorCompraEngine>>();
            _engine = new MotorCompraEngine(_uowMock.Object, _irServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldReturnEarly_IfNotExecutionDay()
        {
            // Arrange
            var dataNaoAlvo = new DateTime(2026, 3, 1); // 01/03/2026 é domingo, não é dia de execução (5, 15, 25)

            // Act
            await _engine.ExecutarProcessamentoDiarioAsync(dataNaoAlvo);

            // Assert
            _uowMock.Verify(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldReturnEarly_IfNoActiveBasket()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5); // 05/03/2026 é quinta-feira
            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()))
                    .ReturnsAsync((CestaRecomendacao)null);

            // Act
            await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            _uowMock.Verify(x => x.ItensCesta.GetByCestaIdAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldProcessCorrectly_OnValidDay()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5);
            var cesta = new CestaRecomendacao("Cesta Teste").SetId(1);
            var item = new ItemCesta(1, "PETR4", 100).SetId(10);
            var cliente = new Cliente("Cliente 1", CpfUtils.GerarCpfRelativo(100), "c1@email.com", 3000m).SetId(100);
            var master = new Cliente("Master", CpfUtils.GerarCpfRelativo(1), "master@itau.com", 0).SetId(1);
            var contaMaster = new ContaGrafica(master.Id, "MASTER-001", ContaTipo.MASTER).SetId(50);
            var contaFilhote = new ContaGrafica(cliente.Id, "FLH-001", ContaTipo.FILHOTE).SetId(60);
            var cotacao = new Cotacao(dataAlvo.AddDays(-1), "PETR4", 30.00m, 35.00m, 36.00m, 29.00m);

            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync(cesta);
            _uowMock.Setup(x => x.ItensCesta.GetByCestaIdAsync(1)).ReturnsAsync(new List<ItemCesta> { item });
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>())).ReturnsAsync(contaMaster);
            _uowMock.Setup(x => x.Clientes.ToListAsync(It.IsAny<Expression<Func<Cliente, bool>>>())).ReturnsAsync(new List<Cliente> { cliente });
            _uowMock.Setup(x => x.Cotacoes.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync(cotacao);
            _uowMock.Setup(x => x.Custodias.GetByContaGraficaIdAsync(contaMaster.Id)).ReturnsAsync(new List<Custodia>()); // Custodia master vazia
            _uowMock.Setup(x => x.Custodias.GetByContaGraficaIdAsync(contaFilhote.Id)).ReturnsAsync(new List<Custodia>());
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.Is<Expression<Func<ContaGrafica, bool>>>(e => e.Compile()(contaFilhote)))).ReturnsAsync(contaFilhote);
            
            // Mock for DistribuirParaClientesAsync
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>()))
                .ReturnsAsync((Expression<Func<ContaGrafica, bool>> predicate) => 
                {
                    if (predicate.Compile()(contaMaster)) return contaMaster;
                    if (predicate.Compile()(contaFilhote)) return contaFilhote;
                    return null;
                });

            _uowMock.Setup(x => x.Custodias.FirstOrDefaultAsync(It.IsAny<Expression<Func<Custodia, bool>>>()))
                .ReturnsAsync(new Custodia(contaMaster.Id, "PETR4", 0, 35m));

            // Act
            await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            // Aporte diário = 3000 / 3 = 1000.
            // Qtd PETR4 = 1000 / 35 = 28.57 -> 28 acoes.
            _uowMock.Verify(x => x.Ordens.AddAsync(It.Is<OrdemCompra>(o => o.Quantidade == 28 && o.Ticker == "PETR4F")), Times.Once);
            _uowMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldReturnEarly_IfBasketHasNoItems()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5);
            var cesta = new CestaRecomendacao("Cesta Vazia").SetId(1);
            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync(cesta);
            _uowMock.Setup(x => x.ItensCesta.GetByCestaIdAsync(1)).ReturnsAsync(new List<ItemCesta>());

            // Act
            await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            _uowMock.Verify(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldHandleWeekendAdjustment()
        {
            // Arrange
            // 15/03/2026 é domingo. Próximo dia útil deve ser 16/03/2026 (segunda-feira).
            var dataDomingo = new DateTime(2026, 3, 15);
            var dataSegunda = new DateTime(2026, 3, 16);
            
            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync((CestaRecomendacao)null);

            // Act & Assert
            await _engine.ExecutarProcessamentoDiarioAsync(dataDomingo);
            _uowMock.Verify(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()), Times.Never);

            await _engine.ExecutarProcessamentoDiarioAsync(dataSegunda);
            _uowMock.Verify(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldCreateLoteAndFracionario_WhenNeeded()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5);
            var cesta = new CestaRecomendacao("Cesta Teste").SetId(1);
            var item = new ItemCesta(1, "PETR4", 100).SetId(10);
            var cliente = new Cliente("Cliente 1", CpfUtils.GerarCpfRelativo(200), "c1@email.com", 12600m).SetId(100); // 12600 / 3 = 4200
            var master = new Cliente("Master", CpfUtils.GerarCpfRelativo(1), "master@itau.com", 0).SetId(1);
            var contaMaster = new ContaGrafica(master.Id, "MASTER-001", ContaTipo.MASTER).SetId(50);
            var cotacao = new Cotacao(dataAlvo.AddDays(-1), "PETR4", 30.00m, 30.00m, 30.00m, 30.00m);

            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync(cesta);
            _uowMock.Setup(x => x.ItensCesta.GetByCestaIdAsync(1)).ReturnsAsync(new List<ItemCesta> { item });
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>())).Returns(async (Expression<Func<ContaGrafica, bool>> p) => {
                var c = new List<ContaGrafica> { contaMaster };
                return c.AsQueryable().FirstOrDefault(p);
            });
            _uowMock.Setup(x => x.Clientes.ToListAsync(It.IsAny<Expression<Func<Cliente, bool>>>())).ReturnsAsync(new List<Cliente> { cliente });
            _uowMock.Setup(x => x.Cotacoes.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync(cotacao);
            _uowMock.Setup(x => x.Custodias.GetByContaGraficaIdAsync(It.IsAny<long>())).ReturnsAsync(new List<Custodia>());
            
            // Act
            // Qtd alvo = 4200 / 30 = 140. 
            // 100 Lote, 40 Fracionario.
            await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            _uowMock.Verify(x => x.Ordens.AddAsync(It.Is<OrdemCompra>(o => o.TipoMercado == TipoMercado.LOTE && o.Quantidade == 100)), Times.Once);
            _uowMock.Verify(x => x.Ordens.AddAsync(It.Is<OrdemCompra>(o => o.TipoMercado == TipoMercado.FRACIONARIO && o.Quantidade == 40 && o.Ticker == "PETR4F")), Times.Once);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldSkipItem_IfPriceIsInvalid()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5);
            var cesta = new CestaRecomendacao("Cesta Teste").SetId(1);
            var item = new ItemCesta(1, "PETR4", 100).SetId(10);
            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync(cesta);
            _uowMock.Setup(x => x.ItensCesta.GetByCestaIdAsync(1)).ReturnsAsync(new List<ItemCesta> { item });
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>())).ReturnsAsync(new ContaGrafica(1, "M", ContaTipo.MASTER));
            _uowMock.Setup(x => x.Clientes.ToListAsync(It.IsAny<Expression<Func<Cliente, bool>>>())).ReturnsAsync(new List<Cliente> { new Cliente("C", CpfUtils.GerarCpfRelativo(250), "e", 1000) });
            _uowMock.Setup(x => x.Cotacoes.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync(new Cotacao(DateTime.Now, "PETR4", 0, 0, 0, 0)); // Preço 0

            // Act
            await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            _uowMock.Verify(x => x.Ordens.AddAsync(It.IsAny<OrdemCompra>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldNotBuy_IfMasterHasEnoughStock()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5);
            var cesta = new CestaRecomendacao("Cesta Teste").SetId(1);
            var item = new ItemCesta(1, "PETR4", 100).SetId(10);
            var contaMaster = new ContaGrafica(1, "M", ContaTipo.MASTER).SetId(50);
            
            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync(cesta);
            _uowMock.Setup(x => x.ItensCesta.GetByCestaIdAsync(1)).ReturnsAsync(new List<ItemCesta> { item });
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>())).ReturnsAsync(contaMaster);
            _uowMock.Setup(x => x.Clientes.ToListAsync(It.IsAny<Expression<Func<Cliente, bool>>>())).ReturnsAsync(new List<Cliente> { new Cliente("C", CpfUtils.GerarCpfRelativo(250), "e", 300) }); // Aporte 100
            
            _uowMock.Setup(x => x.Cotacoes.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync(new Cotacao(DateTime.Now, "PETR4", 10, 10, 10, 10)); // Qtd Alvo = 100/10 = 10
            
            // Master já tem 20 (mais que os 10 necessários)
            _uowMock.Setup(x => x.Custodias.GetByContaGraficaIdAsync(50)).ReturnsAsync(new List<Custodia> { new Custodia(50, "PETR4", 20, 10) });

            // Act
            await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            _uowMock.Verify(x => x.Ordens.AddAsync(It.IsAny<OrdemCompra>()), Times.Never);
        }
        [Fact]
        public async Task ExecutarProcessamentoDiarioAsync_ShouldReturnDetailedResponse_OnSuccess()
        {
            // Arrange
            var dataAlvo = new DateTime(2026, 3, 5);
            var cesta = new CestaRecomendacao("Cesta Teste").SetId(1);
            var item = new ItemCesta(1, "PETR4", 100).SetId(10);
            var cliente = new Cliente("Cliente 1", CpfUtils.GerarCpfRelativo(100), "c1@email.com", 3000m).SetId(100);
            var master = new Cliente("Master", CpfUtils.GerarCpfRelativo(1), "master@itau.com", 0).SetId(1);
            var contaMaster = new ContaGrafica(master.Id, "MASTER-001", ContaTipo.MASTER).SetId(50);
            var contaFilhote = new ContaGrafica(cliente.Id, "FLH-001", ContaTipo.FILHOTE).SetId(60);
            var cotacao = new Cotacao(dataAlvo.AddDays(-1), "PETR4", 30.00m, 35.00m, 36.00m, 29.00m);

            _uowMock.Setup(x => x.Cestas.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>())).ReturnsAsync(cesta);
            _uowMock.Setup(x => x.ItensCesta.GetByCestaIdAsync(1)).ReturnsAsync(new List<ItemCesta> { item });
            _uowMock.Setup(x => x.Contas.FirstOrDefaultAsync(It.IsAny<Expression<Func<ContaGrafica, bool>>>()))
                .ReturnsAsync((Expression<Func<ContaGrafica, bool>> predicate) => 
                {
                    if (predicate.Compile()(contaMaster)) return contaMaster;
                    if (predicate.Compile()(contaFilhote)) return contaFilhote;
                    return null;
                });
            _uowMock.Setup(x => x.Clientes.ToListAsync(It.IsAny<Expression<Func<Cliente, bool>>>())).ReturnsAsync(new List<Cliente> { cliente });
            _uowMock.Setup(x => x.Cotacoes.GetUltimaCotacaoAsync("PETR4")).ReturnsAsync(cotacao);
            _uowMock.Setup(x => x.Custodias.GetByContaGraficaIdAsync(contaMaster.Id)).ReturnsAsync(new List<Custodia>());
            _uowMock.Setup(x => x.Custodias.GetByContaGraficaIdAsync(contaFilhote.Id)).ReturnsAsync(new List<Custodia>());
            
            _uowMock.Setup(x => x.Custodias.FirstOrDefaultAsync(It.IsAny<Expression<Func<Custodia, bool>>>()))
                .ReturnsAsync(new Custodia(contaMaster.Id, "PETR4", 0, 35m));

            // Act
            var result = await _engine.ExecutarProcessamentoDiarioAsync(dataAlvo);

            // Assert
            result.Should().NotBeNull();
            result.TotalClientes.Should().Be(1);
            result.OrdensCompra.Should().HaveCount(1);
            result.OrdensCompra.First().Ticker.Should().Be("PETR4");
            result.Distribuicoes.Should().HaveCount(1);
            result.Distribuicoes.First().ClienteId.Should().Be(cliente.Id);
            result.Mensagem.Should().Contain("sucesso para 1 clientes");
        }
    }
}

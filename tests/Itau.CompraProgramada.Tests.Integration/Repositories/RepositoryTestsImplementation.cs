using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Infrastructure.Repositories;
using Itau.CompraProgramada.Domain.Utils;
using FluentAssertions;
using Xunit;

namespace Itau.CompraProgramada.Tests.Integration.Repositories
{
    // Testes para ClienteRepository
    public class ClienteRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<Cliente>(dbFixture)
    {
        protected override Cliente CreateTestEntity() => new("Teste", CpfUtils.GerarCpfRelativo(100), "teste@repo.com", 1000);
        protected override void UpdateEntity(Cliente entity) => typeof(Cliente).GetProperty("Nome")?.SetValue(entity, "Nome Alterado");
        protected override void VerifyUpdate(Cliente entity) => entity.Nome.Should().Be("Nome Alterado");
        protected override Type GetRepositoryType() => typeof(ClienteRepository);
    }

    // Testes para CestaRecomendacaoRepository
    public class CestaRecomendacaoRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<CestaRecomendacao>(dbFixture)
    {
        protected override CestaRecomendacao CreateTestEntity() => new("Cesta Teste");
        protected override void UpdateEntity(CestaRecomendacao entity) => typeof(CestaRecomendacao).GetProperty("Nome")?.SetValue(entity, "Cesta Alterada");
        protected override void VerifyUpdate(CestaRecomendacao entity) => entity.Nome.Should().Be("Cesta Alterada");
        protected override Type GetRepositoryType() => typeof(CestaRecomendacaoRepository);
    }

    // Testes para CotacaoRepository
    public class CotacaoRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<Cotacao>(dbFixture)
    {
        protected override Cotacao CreateTestEntity() => new(DateTime.UtcNow, "ITUB4", 30m, 31m, 32m, 29m);
        protected override void UpdateEntity(Cotacao entity) => typeof(Cotacao).GetProperty("Ticker")?.SetValue(entity, "PETR4");
        protected override void VerifyUpdate(Cotacao entity) => entity.Ticker.Should().Be("PETR4");
        protected override Type GetRepositoryType() => typeof(CotacaoRepository);

        [Fact]
        public async Task GetUltimaCotacaoAsync_ShouldReturnMostRecent()
        {
            // Arrange
            var ticker = "TESTE_ULTIMA";
            var oldest = new Cotacao(DateTime.UtcNow.AddDays(-2), ticker, 10m, 11m, 12m, 9m);
            var newest = new Cotacao(DateTime.UtcNow.AddDays(-1), ticker, 20m, 21m, 22m, 19m);
            
            Context.Cotacoes.AddRange(oldest, newest);
            await Context.SaveChangesAsync();

            var repository = new CotacaoRepository(Context);

            // Act
            var result = await repository.GetUltimaCotacaoAsync(ticker);

            // Assert
            result.Should().NotBeNull();
            result!.DataPregao.Should().Be(newest.DataPregao);
        }
    }

    // Testes para RebalanceamentoRepository
    public class RebalanceamentoRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<Rebalanceamento>(dbFixture)
    {
        private long _clienteId;
        protected override async Task PrepareDependenciesAsync()
        {
            var cliente = new Cliente("Teste Rebal", CpfUtils.GerarCpfRelativo(200), "teste@rebal.com", 1000);
            Context.Clientes.Add(cliente);
            await Context.SaveChangesAsync();
            _clienteId = cliente.Id;
        }

        protected override Rebalanceamento CreateTestEntity() => new(_clienteId, Domain.Enums.RebalanceamentoTipo.MUDANCA_CESTA, "ITUB4", "PETR4", 3000m);
        protected override void UpdateEntity(Rebalanceamento entity) => typeof(Rebalanceamento).GetProperty("TickerVendido")?.SetValue(entity, "VALE3");
        protected override void VerifyUpdate(Rebalanceamento entity) => entity.TickerVendido.Should().Be("VALE3");
        protected override Type GetRepositoryType() => typeof(RebalanceamentoRepository);
    }

    // Testes para ContaGraficaRepository
    public class ContaGraficaRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<ContaGrafica>(dbFixture)
    {
        private long _clienteId;
        protected override async Task PrepareDependenciesAsync()
        {
            var cliente = new Cliente("Teste Conta", CpfUtils.GerarCpfRelativo(300), "teste@conta.com", 1000);
            Context.Clientes.Add(cliente);
            await Context.SaveChangesAsync();
            _clienteId = cliente.Id;
        }

        protected override ContaGrafica CreateTestEntity() => new(_clienteId, "12345-6", Domain.Enums.ContaTipo.MASTER);
        protected override void UpdateEntity(ContaGrafica entity) => typeof(ContaGrafica).GetProperty("NumeroConta")?.SetValue(entity, "99999-1");
        protected override void VerifyUpdate(ContaGrafica entity) => entity.NumeroConta.Should().Be("99999-1");
        protected override Type GetRepositoryType() => typeof(ContaGraficaRepository);
    }

    // Testes para CustodiaRepository
    public class CustodiaRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<Custodia>(dbFixture)
    {
        private long _contaId;
        protected override async Task PrepareDependenciesAsync()
        {
            var cliente = new Cliente("Teste Cust", CpfUtils.GerarCpfRelativo(400), "teste@cust.com", 1000);
            Context.Clientes.Add(cliente);
            await Context.SaveChangesAsync();
            var conta = new ContaGrafica(cliente.Id, "11111-1", Domain.Enums.ContaTipo.MASTER);
            Context.ContasGraficas.Add(conta);
            await Context.SaveChangesAsync();
            _contaId = conta.Id;
        }

        protected override Custodia CreateTestEntity() => new(_contaId, "ITUB4", 10, 25.5m);
        protected override void UpdateEntity(Custodia entity) => entity.AtualizarPosicao(20, 26.0m);
        protected override void VerifyUpdate(Custodia entity) => entity.Quantidade.Should().Be(20);
        protected override Type GetRepositoryType() => typeof(CustodiaRepository);
    }

    // Testes para ItemCestaRepository
    public class ItemCestaRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<ItemCesta>(dbFixture)
    {
        private long _cestaId;
        protected override async Task PrepareDependenciesAsync()
        {
            var cesta = new CestaRecomendacao("Cesta Dep");
            Context.CestasRecomendacao.Add(cesta);
            await Context.SaveChangesAsync();
            _cestaId = cesta.Id;
        }

        protected override ItemCesta CreateTestEntity() => new(_cestaId, "ITUB4", 100.0m);
        protected override void UpdateEntity(ItemCesta entity) => typeof(ItemCesta).GetProperty("Percentual")?.SetValue(entity, 50.0m);
        protected override void VerifyUpdate(ItemCesta entity) => entity.Percentual.Should().Be(50.0m);
        protected override Type GetRepositoryType() => typeof(ItemCestaRepository);
    }

    // Testes para EventoIRRepository
    public class EventoIRRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<EventoIR>(dbFixture)
    {
        private long _clienteId;
        protected override async Task PrepareDependenciesAsync()
        {
            var cliente = new Cliente("Teste IR", CpfUtils.GerarCpfRelativo(500), "teste@ir.com", 1000);
            Context.Clientes.Add(cliente);
            await Context.SaveChangesAsync();
            _clienteId = cliente.Id;
        }

        protected override EventoIR CreateTestEntity() => new(_clienteId, Domain.Enums.EventoIRTipo.IR_VENDA, 1000m, 5m);
        protected override void UpdateEntity(EventoIR entity) => typeof(EventoIR).GetProperty("ValorBase")?.SetValue(entity, 2000m);
        protected override void VerifyUpdate(EventoIR entity) => entity.ValorBase.Should().Be(2000m);
        protected override Type GetRepositoryType() => typeof(EventoIRRepository);
    }

    // Testes para LogRepository
    public class LogRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<Log>(dbFixture)
    {
        protected override Log CreateTestEntity() => new("INFO", "Teste de log", "Excecao", "Origem");
        protected override void UpdateEntity(Log entity) => typeof(Log).GetProperty("Nivel")?.SetValue(entity, "ERROR");
        protected override void VerifyUpdate(Log entity) => entity.Nivel.Should().Be("ERROR");
        protected override Type GetRepositoryType() => typeof(LogRepository);
    }

    // Testes para OrdemCompraRepository
    public class OrdemCompraRepositoryTests(DatabaseFixture dbFixture) : GenericRepositoryTestsBase<OrdemCompra>(dbFixture)
    {
        private long _contaId;
        protected override async Task PrepareDependenciesAsync()
        {
            var cliente = new Cliente("ITA CORRETORA MASTER", CpfUtils.GerarCpfRelativo(600), "teste@ordem.com", 1000);
            Context.Clientes.Add(cliente);
            await Context.SaveChangesAsync();
            var conta = new ContaGrafica(cliente.Id, "99991-9", Domain.Enums.ContaTipo.MASTER);
            Context.ContasGraficas.Add(conta);
            await Context.SaveChangesAsync();
            _contaId = conta.Id;
        }

        protected override OrdemCompra CreateTestEntity() => new(_contaId, "ITUB4", 100, 30.5m, Domain.Enums.TipoMercado.LOTE);
        protected override void UpdateEntity(OrdemCompra entity) => typeof(OrdemCompra).GetProperty("Ticker")?.SetValue(entity, "VALE3");
        protected override void VerifyUpdate(OrdemCompra entity) => entity.Ticker.Should().Be("VALE3");
        protected override Type GetRepositoryType() => typeof(OrdemCompraRepository);
    }
}

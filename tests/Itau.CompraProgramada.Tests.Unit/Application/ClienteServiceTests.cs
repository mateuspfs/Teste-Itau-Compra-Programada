using FluentAssertions;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Services;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Moq;
using System.Linq.Expressions;
using Itau.CompraProgramada.Tests.Unit.Helpers;

namespace Itau.CompraProgramada.Tests.Unit.Application
{
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly Mock<IContaGraficaRepository> _contaRepoMock;
        private readonly Mock<IHistoricoValorMensalRepository> _histRepoMock;
        private readonly Mock<ICestaRecomendacaoRepository> _cestaRepoMock;
        private readonly Mock<IItemCestaRepository> _itemRepoMock;
        private readonly Mock<ICustodiaRepository> _custodiaRepoMock;
        private readonly Mock<ICotacaoRepository> _cotacaoRepoMock;
        private readonly ClienteService _service;

        public ClienteServiceTests()
        {
            _clienteRepoMock = new Mock<IClienteRepository>();
            _contaRepoMock = new Mock<IContaGraficaRepository>();
            _histRepoMock = new Mock<IHistoricoValorMensalRepository>();
            _cestaRepoMock = new Mock<ICestaRecomendacaoRepository>();
            _itemRepoMock = new Mock<IItemCestaRepository>();
            _custodiaRepoMock = new Mock<ICustodiaRepository>();
            _cotacaoRepoMock = new Mock<ICotacaoRepository>();

            _service = new ClienteService(
                _clienteRepoMock.Object,
                _contaRepoMock.Object,
                _histRepoMock.Object,
                _cestaRepoMock.Object,
                _itemRepoMock.Object,
                _custodiaRepoMock.Object,
                _cotacaoRepoMock.Object);
        }

        [Fact]
        public async Task AderirAoProdutoAsync_ShouldCreateClientAndAccount()
        {
            // Arrange
            var request = new AdesaoClienteRequest 
            { 
                Nome = "Teste", 
                CPF = "52998224725", 
                Email = "t@e.com", 
                ValorMensal = 300m 
            };

            _clienteRepoMock.Setup(x => x.GetByCpfAsync(It.IsAny<string>())).ReturnsAsync((Cliente)null);
            _cestaRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<CestaRecomendacao, bool>>>()))
                          .ReturnsAsync(new CestaRecomendacao("Cesta").SetId(1));
            _itemRepoMock.Setup(x => x.GetByCestaIdAsync(1)).ReturnsAsync([]);

            // Act
            var response = await _service.AderirAoProdutoAsync(request);

            // Assert
            response.Nome.Should().Be("Teste");
            _clienteRepoMock.Verify(x => x.AddAsync(It.IsAny<Cliente>()), Times.Once);
            _contaRepoMock.Verify(x => x.AddAsync(It.Is<ContaGrafica>(c => c.Tipo == ContaTipo.FILHOTE)), Times.Once);
        }

        [Fact]
        public async Task AderirAoProdutoAsync_ShouldFail_IfCpfInvalid()
        {
            // Arrange
            var request = new AdesaoClienteRequest { CPF = "123" };

            // Act
            var act = async () => await _service.AderirAoProdutoAsync(request);

            // Assert
            await act.Should().ThrowAsync<Itau.CompraProgramada.Application.Exceptions.ValidationException>()
                     .WithMessage("*CPF informado é inválido*");
        }

        [Fact]
        public async Task AderirAoProdutoAsync_ShouldFail_IfValorMensalTooLow()
        {
            // Arrange
            var request = new AdesaoClienteRequest { CPF = "52998224725", ValorMensal = 50 };

            // Act
            var act = async () => await _service.AderirAoProdutoAsync(request);

            // Assert
            await act.Should().ThrowAsync<Itau.CompraProgramada.Application.Exceptions.ValidationException>()
                     .WithMessage("*valor mensal minímo*");
        }

        [Fact]
        public async Task AderirAoProdutoAsync_ShouldFail_IfCpfDuplicate()
        {
            // Arrange
            var request = new AdesaoClienteRequest { CPF = "52998224725", ValorMensal = 300 };
            _clienteRepoMock.Setup(x => x.GetByCpfAsync(It.IsAny<string>()))
                            .ReturnsAsync(new Cliente("Existing", "52998224725", "e@e.com", 300));

            // Act
            var act = async () => await _service.AderirAoProdutoAsync(request);

            // Assert
            await act.Should().ThrowAsync<Itau.CompraProgramada.Application.Exceptions.ValidationException>()
                     .WithMessage("*CPF já cadastrado*");
        }

        [Fact]
        public async Task SairDoProdutoAsync_ShouldThrowNotFound_IfClientMissing()
        {
            // Arrange
            _clienteRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Cliente)null);

            // Act
            var act = async () => await _service.SairDoProdutoAsync(1);

            // Assert
            await act.Should().ThrowAsync<Itau.CompraProgramada.Application.Exceptions.NotFoundException>();
        }

        [Fact]
        public async Task SairDoProdutoAsync_ShouldFail_IfClientAlreadyInactive()
        {
            // Arrange
            var cliente = new Cliente("Teste", "52998224725", "e@e.com", 300);
            cliente.Desativar();
            _clienteRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);

            // Act
            var act = async () => await _service.SairDoProdutoAsync(1);

            // Assert
            await act.Should().ThrowAsync<Itau.CompraProgramada.Application.Exceptions.ValidationException>()
                     .WithMessage("*já havia saído do produto*");
        }

        [Fact]
        public async Task ObterRentabilidadeAsync_ShouldThrowNotFound_IfAccountMissing()
        {
            // Arrange
            var cliente = new Cliente("Teste", "52998224725", "e@e.com", 300).SetId(1);
            _clienteRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);
            _contaRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ContaGrafica>());

            // Act
            var act = async () => await _service.ObterRentabilidadeAsync(1);

            // Assert
            await act.Should().ThrowAsync<Itau.CompraProgramada.Application.Exceptions.NotFoundException>()
                     .WithMessage("*Conta gráfica não encontrada*");
        }

        [Fact]
        public async Task ObterRentabilidadeAsync_ShouldHandleZeroInvested_Correctly()
        {
            // Arrange
            var cliente = new Cliente("Teste", "52998224725", "e@e.com", 300).SetId(1);
            _clienteRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);
            
            var conta = new ContaGrafica(1, "FLH-001", ContaTipo.FILHOTE).SetId(10);
            _contaRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ContaGrafica> { conta });
            _custodiaRepoMock.Setup(x => x.GetByContaGraficaIdAsync(10)).ReturnsAsync(new List<Custodia>());

            // Act
            var response = await _service.ObterRentabilidadeAsync(1);

            // Assert
            response.Rentabilidade.RentabilidadePercentual.Should().Be(0);
            response.Rentabilidade.ValorTotalInvestido.Should().Be(0);
        }
    }
}

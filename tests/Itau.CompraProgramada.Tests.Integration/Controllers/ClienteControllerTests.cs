using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Domain.Utils;

namespace Itau.CompraProgramada.Tests.Integration.Controllers
{
    [Collection("Database collection")]
    public class ClienteControllerTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
    {
        private readonly HttpClient _client = fixture.CreateClient();

        [Fact]
        public async Task Aderir_ShouldReturnCreated_WhenDataIsValid()
        {
            // Arrange
            var request = new AdesaoClienteRequest
            {
                Nome = "Joao Silva",
                CPF = CpfUtils.GerarCpfRelativo(401),
                Email = "joao@teste.com",
                ValorMensal = 1000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Clientes/adesao", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Aderir_ShouldReturnBadRequest_WhenCpfIsInvalid()
        {
            // Arrange
            var request = new AdesaoClienteRequest
            {
                Nome = "Joao Silva",
                CPF = "123",
                Email = "joao@teste.com",
                ValorMensal = 1000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Clientes/adesao", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ObterCarteira_ShouldReturnOk_WhenClienteExists()
        {
            // Arrange
            var request = new AdesaoClienteRequest
            {
                Nome = "Maria Oliveira",
                CPF = CpfUtils.GerarCpfRelativo(501),
                Email = "maria@teste.com",
                ValorMensal = 500
            };
            var adesaoResponse = await _client.PostAsJsonAsync("/api/Clientes/adesao", request);
            var adesaoData = await adesaoResponse.Content.ReadFromJsonAsync<AdesaoClienteResponse>();

            // Act
            var response = await _client.GetAsync($"/api/Clientes/{adesaoData!.ClienteId}/carteira");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        public async Task SairDoProduto_ShouldReturnOk_WhenClienteExists()
        {
            // Arrange
            var request = new AdesaoClienteRequest
            {
                Nome = "Pedro Santos",
                CPF = CpfUtils.GerarCpfRelativo(601),
                Email = "pedro@teste.com",
                ValorMensal = 2000
            };
            var adesaoResponse = await _client.PostAsJsonAsync("/api/Clientes/adesao", request);
            var adesaoData = await adesaoResponse.Content.ReadFromJsonAsync<AdesaoClienteResponse>();

            // Act
            var response = await _client.PostAsync($"/api/Clientes/{adesaoData!.ClienteId}/saida", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AlterarValorMensal_ShouldReturnOk_WhenDataIsEqualOrGreater()
        {
            // Arrange
            var request = new AdesaoClienteRequest
            {
                Nome = "Ana Costa",
                CPF = CpfUtils.GerarCpfRelativo(701),
                Email = "ana@teste.com",
                ValorMensal = 1000
            };
            var adesaoResponse = await _client.PostAsJsonAsync("/api/Clientes/adesao", request);
            var adesaoData = await adesaoResponse.Content.ReadFromJsonAsync<AdesaoClienteResponse>();
            
            var updateRequest = new AlterarValorMensalRequest { NovoValorMensal = 1500 };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Clientes/{adesaoData!.ClienteId}/valor-mensal", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
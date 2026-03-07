using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Itau.CompraProgramada.Domain.Entities;

namespace Itau.CompraProgramada.Tests.Integration.Controllers
{
    [Collection("Database collection")]
    public class AdminControllerTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
    {
        private readonly HttpClient _client = fixture.CreateClient();

        [Fact]
        public async Task CadastrarAlterarCesta_DeveRetornarCreated_QuandoDadosSaoValidos()
        {
            // Arrange
            var request = new {
                Nome = "Cesta de Teste " + Guid.NewGuid().ToString().Substring(0,8),
                Itens = new[] {
                    new { Ticker = "ITUB4", Percentual = 10.0m },
                    new { Ticker = "PETR4", Percentual = 10.0m },
                    new { Ticker = "VALE3", Percentual = 30.0m },
                    new { Ticker = "BBDC4", Percentual = 25.0m },
                    new { Ticker = "WEGE3", Percentual = 25.0m },
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Admin/cesta", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task ObterCestaAtual_DeveRetornarOk()
        {
            // Act
            var response = await _client.GetAsync("/api/Admin/cesta/atual");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ObterHistoricoCestas_DeveRetornarOk()
        {
            // Act
            var response = await _client.GetAsync("/api/Admin/cesta/historico");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

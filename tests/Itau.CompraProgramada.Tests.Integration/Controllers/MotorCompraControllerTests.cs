using System.Net;
using FluentAssertions;
using Xunit;

namespace Itau.CompraProgramada.Tests.Integration.Controllers
{
    [Collection("Database collection")]
    public class MotorCompraControllerTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
    {
        private readonly HttpClient _client = fixture.CreateClient();

        [Fact]
        public async Task ExecutarTeste_ShouldReturnOk_WithSpecificDate()
        {
            // Arrange
            var data = "2026-03-05";

            // Act
            var response = await _client.PostAsync($"/api/MotorCompra/executar-teste?data={data}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ExecutarTeste_ShouldReturnOk_WithInvalidDate()
        {
            // Arrange
            var data = "invalid-date";

            // Act
            var response = await _client.PostAsync($"/api/MotorCompra/executar-teste?data={data}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

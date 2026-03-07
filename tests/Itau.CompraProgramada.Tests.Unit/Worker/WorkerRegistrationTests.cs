using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Itau.CompraProgramada.Worker;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Interfaces.Processor;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Itau.CompraProgramada.Application.DTOs.Admin;

namespace Itau.CompraProgramada.Tests.Unit.Worker
{
    public class WorkerRegistrationTests
    {
        [Fact]
        public void Services_ShouldBeRegisteredCorrectly()
        {
            // Arrange
            var builder = Host.CreateApplicationBuilder();
            
            // Simular o que está no Program.cs
            builder.Services.AddScoped<ICotacaoProcessor, MockCotacaoProcessor>();
            builder.Services.AddScoped<IMotorCompraEngine, MockMotorCompraEngine>();
            builder.Services.AddHostedService<Itau.CompraProgramada.Worker.Worker>();

            // Act
            var host = builder.Build();
            var worker = host.Services.GetServices<IHostedService>().FirstOrDefault(s => s is Itau.CompraProgramada.Worker.Worker);

            // Assert
            worker.Should().NotBeNull();
        }

        private class MockCotacaoProcessor : ICotacaoProcessor 
        { 
            public Task<int> ProcessarArquivoAsync(string filePath) => Task.FromResult(0);
            public Task<decimal> ObterCotacaoAtualAsync(string ticker) => Task.FromResult(0m);
        }

        private class MockMotorCompraEngine : IMotorCompraEngine
        {
            public Task<MotorCompraResponse> ExecutarProcessamentoDiarioAsync(DateTime dataReferencia) => 
                Task.FromResult(new MotorCompraResponse());
        }
    }
}

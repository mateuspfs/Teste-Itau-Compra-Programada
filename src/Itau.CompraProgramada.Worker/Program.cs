using Itau.CompraProgramada.Worker;
using Itau.CompraProgramada.Worker.Services;
using Itau.CompraProgramada.Infrastructure;
using Itau.CompraProgramada.Application;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Interfaces.Processor;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddScoped<ICotacaoProcessor, CotacaoProcessor>();
builder.Services.AddScoped<IMotorCompraEngine, MotorCompraEngine>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

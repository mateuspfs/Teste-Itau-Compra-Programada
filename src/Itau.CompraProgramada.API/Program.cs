using Itau.CompraProgramada.API.Middlewares;
using Itau.CompraProgramada.Application;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Interfaces.Processor;
using Itau.CompraProgramada.Infrastructure;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Worker;
using Itau.CompraProgramada.Worker.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddScoped<ICotacaoProcessor, CotacaoProcessor>();
builder.Services.AddScoped<IMotorCompraEngine, MotorCompraEngine>();
builder.Services.AddScoped<IInicializadorBanco, InicializadorBanco>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Request-Id");
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CompraProgramada API",
        Version = "v1",
        Description = "API para o Sistema de Compra Programada de Ações da Itaú Corretora. Permite gerenciar adesões, consultar rentabilidade e acompanhar o motor de investimentos recorrentes."
    });

    c.EnableAnnotations();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.Path.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<RequestIdMiddleware>();

// Execução de migrations e seed de dados
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IInicializadorBanco>();
    await initializer.InicializarAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompraProgramada API v1");
    c.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }

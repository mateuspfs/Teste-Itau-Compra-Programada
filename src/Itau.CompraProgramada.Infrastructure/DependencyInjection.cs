using Itau.CompraProgramada.Domain.Interfaces.Imports;
using Itau.CompraProgramada.Domain.Interfaces.Generic;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Imports;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itau.CompraProgramada.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddDbContext(services, configuration);
            AddRepositories(services);
            AddServices(services);
            AddImports(services);

            return services;
        }

        private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IClienteRepository, Repositories.ClienteRepository>();
            services.AddScoped<IContaGraficaRepository, Repositories.ContaGraficaRepository>();
            services.AddScoped<ICustodiaRepository, Repositories.CustodiaRepository>();
            services.AddScoped<ICestaRecomendacaoRepository, Repositories.CestaRecomendacaoRepository>();
            services.AddScoped<IItemCestaRepository, Repositories.ItemCestaRepository>();
            services.AddScoped<IOrdemCompraRepository, Repositories.OrdemCompraRepository>();
            services.AddScoped<IDistribuicaoRepository, Repositories.DistribuicaoRepository>();
            services.AddScoped<IEventoIRRepository, Repositories.EventoIRRepository>();
            services.AddScoped<ICotacaoRepository, Repositories.CotacaoRepository>();
            services.AddScoped<IRebalanceamentoRepository, Repositories.RebalanceamentoRepository>();
            services.AddScoped<ILogRepository, Repositories.LogRepository>();
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddScoped<IInicializadorBanco, InicializadorBanco>();
        }

        private static void AddImports(IServiceCollection services)
        {
            services.AddScoped<ICotacaoImport, CotacaoImport>();
        }
    }
}

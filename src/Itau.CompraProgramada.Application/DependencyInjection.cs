using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itau.CompraProgramada.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IRebalanceamentoService, RebalanceamentoService>();
            services.AddScoped<ICarteiraService, CarteiraService>();
            services.AddScoped<IIRService, IRService>();

            return services;
        }
    }
}

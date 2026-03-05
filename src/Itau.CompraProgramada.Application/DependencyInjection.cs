using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itau.CompraProgramada.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClienteAppService, ClienteAppService>();
            services.AddScoped<IAdminAppService, AdminAppService>();

            return services;
        }
    }
}

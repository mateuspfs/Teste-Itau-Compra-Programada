using Itau.CompraProgramada.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Itau.CompraProgramada.Tests.Integration
{
    public class ApiTestFixture(DatabaseFixture dbFixture) : WebApplicationFactory<Program>
    {
        private readonly DatabaseFixture _dbFixture = dbFixture;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("DatabaseProvider", "MySql");
            builder.UseSetting("SeedData:CotacoesPath", Path.Combine(AppContext.BaseDirectory, "Resources"));

            builder.ConfigureServices(services =>
            {
                // Remove the production DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add Testcontainers DbContext
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(_dbFixture.MySqlContainer.GetConnectionString(), 
                        new MySqlServerVersion(new Version(8, 0, 0))));

                // Run migrations on the test container
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            });
        }
    }
}

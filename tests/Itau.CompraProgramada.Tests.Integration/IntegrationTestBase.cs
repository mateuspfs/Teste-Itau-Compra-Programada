using Itau.CompraProgramada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Itau.CompraProgramada.Tests.Integration
{
    [Collection("Database collection")]
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly ApplicationDbContext Context;
        private bool _disposed;

        protected IntegrationTestBase(DatabaseFixture dbFixture)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseMySql(dbFixture.MySqlContainer.GetConnectionString(), 
                    new MySqlServerVersion(new Version(8, 0, 0)))
                .Options;

            Context = new ApplicationDbContext(options);
            Context.Database.Migrate();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

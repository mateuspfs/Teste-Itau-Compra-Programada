using Testcontainers.MySql;

namespace Itau.CompraProgramada.Tests.Integration
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public MySqlContainer MySqlContainer { get; } = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("compra_programada_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        public async Task InitializeAsync()
        {
            await MySqlContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await MySqlContainer.StopAsync();
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}

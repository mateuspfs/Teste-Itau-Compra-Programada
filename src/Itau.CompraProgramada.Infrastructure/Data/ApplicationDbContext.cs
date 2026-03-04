using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Itau.CompraProgramada.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ContaGrafica> ContasGraficas { get; set; }
        public DbSet<Custodia> Custodias { get; set; }
        public DbSet<CestaRecomendacao> CestasRecomendacao { get; set; }
        public DbSet<ItemCesta> ItensCesta { get; set; }
        public DbSet<OrdemCompra> OrdensCompra { get; set; }
        public DbSet<Distribuicao> Distribuicoes { get; set; }
        public DbSet<EventoIR> EventosIR { get; set; }
        public DbSet<Cotacao> Cotacoes { get; set; }
        public DbSet<Rebalanceamento> Rebalanceamentos { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}

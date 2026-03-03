using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Data
{
    public class CompraProgramadaDbContext : DbContext
    {
        public CompraProgramadaDbContext(DbContextOptions<CompraProgramadaDbContext> options) : base(options) { }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Cliente>().ToTable("Clientes");
            modelBuilder.Entity<ContaGrafica>().ToTable("ContasGraficas");
            modelBuilder.Entity<Custodia>().ToTable("Custodias");
            modelBuilder.Entity<CestaRecomendacao>().ToTable("CestasRecomendacao");
            modelBuilder.Entity<ItemCesta>().ToTable("ItensCesta");
            modelBuilder.Entity<OrdemCompra>().ToTable("OrdensCompra");
            modelBuilder.Entity<Distribuicao>().ToTable("Distribuicoes");
            modelBuilder.Entity<EventoIR>().ToTable("EventosIR");
            modelBuilder.Entity<Cotacao>().ToTable("Cotacoes");
            modelBuilder.Entity<Rebalanceamento>().ToTable("Rebalanceamentos");
        }
    }
}

using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class OrdemCompraConfiguration : IEntityTypeConfiguration<OrdemCompra>
    {
        public void Configure(EntityTypeBuilder<OrdemCompra> builder)
        {
            builder.ToTable("OrdensCompra");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            builder.Property(e => e.PrecoUnitario).HasPrecision(18, 4);
        }
    }
}

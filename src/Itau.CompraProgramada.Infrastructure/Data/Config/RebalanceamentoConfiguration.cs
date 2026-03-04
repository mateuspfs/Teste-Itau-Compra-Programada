using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class RebalanceamentoConfiguration : IEntityTypeConfiguration<Rebalanceamento>
    {
        public void Configure(EntityTypeBuilder<Rebalanceamento> builder)
        {
            builder.ToTable("Rebalanceamentos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.TickerVendido).HasMaxLength(12).IsRequired();
            builder.Property(e => e.TickerComprado).HasMaxLength(12).IsRequired();
            builder.Property(e => e.ValorVenda).HasPrecision(18, 4);

            builder.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

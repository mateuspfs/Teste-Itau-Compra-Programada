using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class CustodiaConfiguration : IEntityTypeConfiguration<Custodia>
    {
        public void Configure(EntityTypeBuilder<Custodia> builder)
        {
            builder.ToTable("Custodias");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            builder.Property(e => e.PrecoMedio).HasPrecision(18, 4);

            builder.HasOne(e => e.ContaGrafica)
                .WithMany()
                .HasForeignKey(e => e.ContaGraficaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

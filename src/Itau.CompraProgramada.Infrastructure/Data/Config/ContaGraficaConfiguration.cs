using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class ContaGraficaConfiguration : IEntityTypeConfiguration<ContaGrafica>
    {
        public void Configure(EntityTypeBuilder<ContaGrafica> builder)
        {
            builder.ToTable("ContasGraficas");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.NumeroConta).HasMaxLength(20).IsRequired();
            builder.HasIndex(e => e.NumeroConta).IsUnique();

            builder.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

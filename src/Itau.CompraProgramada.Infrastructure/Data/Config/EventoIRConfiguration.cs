using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class EventoIRConfiguration : IEntityTypeConfiguration<EventoIR>
    {
        public void Configure(EntityTypeBuilder<EventoIR> builder)
        {
            builder.ToTable("EventosIR");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.ValorBase).HasPrecision(18, 4);
            builder.Property(e => e.ValorIR).HasPrecision(18, 4);

            builder.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

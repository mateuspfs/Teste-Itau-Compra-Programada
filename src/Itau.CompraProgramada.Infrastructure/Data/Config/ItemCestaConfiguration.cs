using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class ItemCestaConfiguration : IEntityTypeConfiguration<ItemCesta>
    {
        public void Configure(EntityTypeBuilder<ItemCesta> builder)
        {
            builder.ToTable("ItensCesta");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            builder.Property(e => e.Percentual).HasPrecision(18, 4);

            builder.HasOne(e => e.Cesta)
                .WithMany()
                .HasForeignKey(e => e.CestaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

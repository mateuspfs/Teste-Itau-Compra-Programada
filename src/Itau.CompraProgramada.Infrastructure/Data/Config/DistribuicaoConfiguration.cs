using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class DistribuicaoConfiguration : IEntityTypeConfiguration<Distribuicao>
    {
        public void Configure(EntityTypeBuilder<Distribuicao> builder)
        {
            builder.ToTable("Distribuicoes");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            builder.Property(e => e.PrecoUnitario).HasPrecision(18, 4);

            builder.HasOne(e => e.OrdemCompra)
                .WithMany()
                .HasForeignKey(e => e.OrdemCompraId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.CustodiaFilhote)
                .WithMany()
                .HasForeignKey(e => e.CustodiaFilhoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

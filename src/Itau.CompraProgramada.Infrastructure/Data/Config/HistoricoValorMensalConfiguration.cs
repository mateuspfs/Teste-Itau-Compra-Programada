using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class HistoricoValorMensalConfiguration : IEntityTypeConfiguration<HistoricoValorMensal>
    {
        public void Configure(EntityTypeBuilder<HistoricoValorMensal> builder)
        {
            builder.ToTable("HistoricoValorMensal");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ValorAnterior)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.ValorNovo)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.DataAlteracao)
                .IsRequired();

            builder.HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

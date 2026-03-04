using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class CotacaoConfiguration : IEntityTypeConfiguration<Cotacao>
    {
        public void Configure(EntityTypeBuilder<Cotacao> builder)
        {
            builder.ToTable("Cotacoes");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            builder.Property(e => e.PrecoAbertura).HasPrecision(18, 4);
            builder.Property(e => e.PrecoFechamento).HasPrecision(18, 4);
            builder.Property(e => e.PrecoMaximo).HasPrecision(18, 4);
            builder.Property(e => e.PrecoMinimo).HasPrecision(18, 4);
        }
    }
}

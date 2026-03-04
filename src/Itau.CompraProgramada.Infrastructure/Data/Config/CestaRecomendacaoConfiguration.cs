using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class CestaRecomendacaoConfiguration : IEntityTypeConfiguration<CestaRecomendacao>
    {
        public void Configure(EntityTypeBuilder<CestaRecomendacao> builder)
        {
            builder.ToTable("CestasRecomendacao");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nome).HasMaxLength(100).IsRequired();
        }
    }
}

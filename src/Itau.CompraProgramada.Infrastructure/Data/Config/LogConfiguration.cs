using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.ToTable("Logs");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nivel).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Mensagem).IsRequired();
            builder.Property(e => e.Excecao).HasColumnType("longtext");
            builder.Property(e => e.Origem).HasMaxLength(100);
        }
    }
}

using Itau.CompraProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itau.CompraProgramada.Infrastructure.Data.Config
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            builder.Property(e => e.CPF).HasMaxLength(11).IsRequired();
            builder.HasIndex(e => e.CPF).IsUnique();
            builder.Property(e => e.Email).HasMaxLength(100).IsRequired();
            builder.Property(e => e.ValorMensal).HasPrecision(18, 4);
        }
    }
}

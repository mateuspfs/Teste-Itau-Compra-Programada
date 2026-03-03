using System;

namespace Itau.CompraProgramada.Domain.Entities
{
    public abstract class Entity
    {
        public long Id { get; protected set; }
        public DateTime DataCriacao { get; protected set; } = DateTime.UtcNow;
    }
}

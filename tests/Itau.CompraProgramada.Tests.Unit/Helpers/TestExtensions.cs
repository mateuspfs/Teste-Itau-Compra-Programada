using System.Reflection;
using Itau.CompraProgramada.Domain.Entities;

namespace Itau.CompraProgramada.Tests.Unit.Helpers
{
    public static class TestExtensions
    {
        public static T SetId<T>(this T entity, long id) where T : Entity
        {
            var property = typeof(Entity).GetProperty(nameof(Entity.Id), BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                property.SetValue(entity, id);
            }
            return entity;
        }
    }
}

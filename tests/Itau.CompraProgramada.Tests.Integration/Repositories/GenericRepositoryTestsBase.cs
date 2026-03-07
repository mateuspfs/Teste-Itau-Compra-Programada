using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Generic;
using FluentAssertions;
using Xunit;

namespace Itau.CompraProgramada.Tests.Integration.Repositories
{
    public abstract class GenericRepositoryTestsBase<T>(DatabaseFixture dbFixture) : IntegrationTestBase(dbFixture) where T : Entity
    {
        protected abstract T CreateTestEntity();
        protected abstract void UpdateEntity(T entity);
        protected virtual Task PrepareDependenciesAsync() => Task.CompletedTask;

        [Fact]
        public async Task CrudBasico_E_Busca_DeveFuncionar()
        {
            // Prepare dependencies (e.g. seed parent entities)
            await PrepareDependenciesAsync();

            // Arrange
            var repo = (IGenericRepository<T>)Activator.CreateInstance(GetRepositoryType(), Context)!;
            var entity = CreateTestEntity();

            // Act 
            // Testando a adição de uma entidade
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            var id = entity.Id;

            // Assert - GetById
            var retrieved = await repo.GetByIdAsync(id);
            retrieved.Should().NotBeNull();

            // Assert 
            // Testando a busca de todos os registros
            var all = await repo.GetAllAsync();
            all.Should().Contain(e => e.Id == id);

            // Act
            // Testando a atualização de uma entidade
            UpdateEntity(retrieved!);
            repo.Update(retrieved!);
            await repo.SaveChangesAsync();

            // Assert
            var updated = await repo.GetByIdAsync(id);
            VerifyUpdate(updated!);

            // Act
            // Testando a remoção de uma entidade
            repo.Remove(updated!);
            await repo.SaveChangesAsync();

            // Assert 
            var deleted = await repo.GetByIdAsync(id);
            deleted.Should().BeNull();
        }

        protected abstract Type GetRepositoryType();
        protected abstract void VerifyUpdate(T entity);
    }
}

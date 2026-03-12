using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IBaseUnitOfWork : IDisposable
{
    public IBaseRepository<TEntity> GetRepository<TEntity>()
        where TEntity : BaseEntity;

    public Task CompleteAsync();
}

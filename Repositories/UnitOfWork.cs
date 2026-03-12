using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;

namespace FamilyTree.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FamilyTreeContext _context;
    private readonly Dictionary<string, object> _repositories = new();

    public UnitOfWork(FamilyTreeContext context)
    {
        _context = context;
    }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    public IBaseRepository<TEntity> GetRepository<TEntity>()
        where TEntity : BaseEntity
    {
        string type = typeof(TEntity).Name;

        if (_repositories.ContainsKey(type))
        {
            return (IBaseRepository<TEntity>)_repositories[type];
        }

        IBaseRepository<TEntity> repository =
            new BaseRepository<TEntity>(_context);

        _repositories.Add(type, repository);

        return repository;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

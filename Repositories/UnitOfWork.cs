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

        Users = new UserRepository(_context);
        Roles = new RoleRepository(_context);
        RefreshTokens = new RefreshTokenRepository(_context);
        FamilyAccesses = new FamilyAccessRepository(_context);
        FamilyInvitations = new FamilyInvitationRepository(_context);
    }

    public IUserRepository Users { get; }

    public IRoleRepository Roles { get; }

    public IRefreshTokenRepository RefreshTokens { get; }

    public IFamilyAccessRepository FamilyAccesses { get; }

    public IFamilyInvitationRepository FamilyInvitations { get; }

    public IBaseRepository<TEntity> GetRepository<TEntity>()
        where TEntity : BaseEntity
    {
        string type = typeof(TEntity).Name;

        if (_repositories.ContainsKey(type))
        {
            return (IBaseRepository<TEntity>)_repositories[type];
        }

        IBaseRepository<TEntity> repository = new BaseRepository<TEntity>(_context);
        _repositories.Add(type, repository);

        return repository;
    }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

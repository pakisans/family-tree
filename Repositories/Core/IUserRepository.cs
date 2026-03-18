using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IUserRepository : IBaseRepository<User>
{
    public Task<User?> GetByEmailAsync(string email);

    public Task<User?> GetByEmailWithRolesAsync(string email);

    public Task<User?> GetWithRolesAsync(long userId);
}

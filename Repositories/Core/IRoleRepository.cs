using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IRoleRepository : IBaseRepository<Role>
{
    public Task<Role?> GetByNameAsync(string name);
}

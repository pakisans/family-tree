using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IFamilyAccessRepository : IBaseRepository<FamilyAccess>
{
    public Task<FamilyAccess?> GetActiveAccessAsync(long familyId, long userId);

    public Task<IList<FamilyAccess>> GetFamilyAccessesAsync(long familyId);
}

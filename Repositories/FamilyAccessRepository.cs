using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class FamilyAccessRepository : BaseRepository<FamilyAccess>, IFamilyAccessRepository
{
    public FamilyAccessRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<FamilyAccess?> GetActiveAccessAsync(long familyId, long userId)
    {
        return await DbContext.FamilyAccesses
            .Include(familyAccess => familyAccess.User)
            .Include(familyAccess => familyAccess.Family)
            .FirstOrDefaultAsync(familyAccess =>
                familyAccess.FamilyId == familyId &&
                familyAccess.UserId == userId &&
                familyAccess.IsActive);
    }

    public async Task<FamilyAccess?> GetAnyAccessAsync(long familyId, long userId)
    {
        return await DbContext.FamilyAccesses
            .FirstOrDefaultAsync(familyAccess =>
                familyAccess.FamilyId == familyId &&
                familyAccess.UserId == userId);
    }

    public async Task<IList<FamilyAccess>> GetFamilyAccessesAsync(long familyId)
    {
        return await DbContext.FamilyAccesses
            .AsNoTracking()
            .Include(familyAccess => familyAccess.User)
            .Where(familyAccess => familyAccess.FamilyId == familyId && familyAccess.IsActive)
            .ToListAsync();
    }
}

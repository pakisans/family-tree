using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IFamilyAccessRepository : IBaseRepository<FamilyAccess>
{
    public Task<FamilyAccess?> GetActiveAccessAsync(long familyId, long userId);

    /// <summary>
    /// Returns any existing access record for this (familyId, userId) pair,
    /// regardless of IsActive. Used during invitation acceptance to handle
    /// the case where a previously removed collaborator is re-invited.
    /// </summary>
    public Task<FamilyAccess?> GetAnyAccessAsync(long familyId, long userId);

    public Task<IList<FamilyAccess>> GetFamilyAccessesAsync(long familyId);
}

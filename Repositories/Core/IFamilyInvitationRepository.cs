using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IFamilyInvitationRepository : IBaseRepository<FamilyInvitation>
{
    public Task<FamilyInvitation?> GetPendingByTokenAsync(string token);
    public Task<IList<FamilyInvitation>> GetFamilyInvitationsAsync(long familyId);
}

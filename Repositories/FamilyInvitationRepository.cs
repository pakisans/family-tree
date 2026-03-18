using FamilyTree.Constants;
using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class FamilyInvitationRepository : BaseRepository<FamilyInvitation>, IFamilyInvitationRepository
{
    public FamilyInvitationRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<FamilyInvitation?> GetPendingByTokenAsync(string token)
    {
        return await DbContext.FamilyInvitations
            .Include(invitation => invitation.Family)
            .FirstOrDefaultAsync(invitation =>
                invitation.Token == token &&
                invitation.Status == InvitationStatus.Pending);
    }

    public async Task<IList<FamilyInvitation>> GetFamilyInvitationsAsync(long familyId)
    {
        return await DbContext.FamilyInvitations
            .AsNoTracking()
            .Where(invitation => invitation.FamilyId == familyId)
            .OrderByDescending(invitation => invitation.DateCreated)
            .ToListAsync();
    }
}

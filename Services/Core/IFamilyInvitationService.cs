using FamilyTree.Dto.Family;

namespace FamilyTree.Services.Core;

public interface IFamilyInvitationService
{
    public Task InviteAsync(long familyId, InviteUserToFamilyRequestDto request);
    public Task<IList<FamilyCollaboratorDto>> GetCollaboratorsAsync(long familyId);
    public Task<IList<FamilyInvitationDto>> GetInvitationsAsync(long familyId);
    public Task RevokeInvitationAsync(long familyId, long invitationId);
    public Task RemoveCollaboratorAsync(long familyId, long collaboratorId);
}

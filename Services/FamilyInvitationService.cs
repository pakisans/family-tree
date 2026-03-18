using System.Net;
using FamilyTree.Constants;
using FamilyTree.Dto.Family;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;
using FamilyTree.Services.Core.Auth;

namespace FamilyTree.Services;

public class FamilyInvitationService : IFamilyInvitationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFamilyAuthorizationService _familyAuthorizationService;
    private readonly ICurrentUserService _currentUserService;

    public FamilyInvitationService(
        IUnitOfWork unitOfWork,
        IFamilyAuthorizationService familyAuthorizationService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _familyAuthorizationService = familyAuthorizationService;
        _currentUserService = currentUserService;
    }

    public async Task InviteAsync(long familyId, InviteUserToFamilyRequestDto request)
    {
        Family? family = await _unitOfWork.GetRepository<Family>().GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."),
                HttpStatusCode.NotFound);
        }

        await _familyAuthorizationService.EnsureCanManageFamilyAccessAsync(family);

        long? invitedByUserId = _currentUserService.GetCurrentUserId();

        if (!invitedByUserId.HasValue)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0004, "Authentication required."),
                HttpStatusCode.Unauthorized);
        }

        FamilyInvitation invitation = new FamilyInvitation
        {
            FamilyId = familyId,
            Email = request.Email.Trim().ToLower(),
            AccessRole = request.AccessRole,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            InvitedByUserId = invitedByUserId.Value,
            Status = InvitationStatus.Pending
        };

        await _unitOfWork.FamilyInvitations.AddAsync(invitation);
    }

    public async Task<IList<FamilyCollaboratorDto>> GetCollaboratorsAsync(long familyId)
    {
        Family? family = await _unitOfWork.GetRepository<Family>().GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."),
                HttpStatusCode.NotFound);
        }

        await _familyAuthorizationService.EnsureCanReadFamilyAsync(family);

        IList<FamilyAccess> accesses = await _unitOfWork.FamilyAccesses.GetFamilyAccessesAsync(familyId);

        return accesses.Select(access => new FamilyCollaboratorDto
        {
            Id = access.Id,
            FamilyId = access.FamilyId,
            UserId = access.UserId,
            UserEmail = access.User?.Email ?? string.Empty,
            UserFirstName = access.User?.FirstName ?? string.Empty,
            UserLastName = access.User?.LastName ?? string.Empty,
            AccessRole = access.AccessRole,
            AcceptedAt = access.AcceptedAt,
            InvitedByUserId = access.InvitedByUserId,
            IsActive = access.IsActive
        }).ToList();
    }

    public async Task<IList<FamilyInvitationDto>> GetInvitationsAsync(long familyId)
    {
        Family? family = await _unitOfWork.GetRepository<Family>().GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."),
                HttpStatusCode.NotFound);
        }

        await _familyAuthorizationService.EnsureCanManageFamilyAccessAsync(family);

        IList<FamilyInvitation> invitations = await _unitOfWork.FamilyInvitations.GetFamilyInvitationsAsync(familyId);

        return invitations.Select(invitation => new FamilyInvitationDto
        {
            Id = invitation.Id,
            FamilyId = invitation.FamilyId,
            Email = invitation.Email,
            AccessRole = invitation.AccessRole,
            Token = invitation.Token,
            ExpiresAt = invitation.ExpiresAt,
            InvitedByUserId = invitation.InvitedByUserId,
            Status = invitation.Status,
            AcceptedAt = invitation.AcceptedAt,
            DateCreated = invitation.DateCreated
        }).ToList();
    }

    public async Task RevokeInvitationAsync(long familyId, long invitationId)
    {
        Family? family = await _unitOfWork.GetRepository<Family>().GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."),
                HttpStatusCode.NotFound);
        }

        await _familyAuthorizationService.EnsureCanManageFamilyAccessAsync(family);

        FamilyInvitation? invitation = await _unitOfWork.FamilyInvitations.GetAsync(invitationId);

        if (invitation == null || invitation.FamilyId != familyId)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Invitation not found."),
                HttpStatusCode.NotFound);
        }

        invitation.Status = InvitationStatus.Revoked;
        await _unitOfWork.CompleteAsync();
    }

    public async Task RemoveCollaboratorAsync(long familyId, long collaboratorId)
    {
        Family? family = await _unitOfWork.GetRepository<Family>().GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."),
                HttpStatusCode.NotFound);
        }

        await _familyAuthorizationService.EnsureCanManageFamilyAccessAsync(family);

        FamilyAccess? access = await _unitOfWork.FamilyAccesses.GetAsync(collaboratorId);

        if (access == null || access.FamilyId != familyId)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Collaborator not found."),
                HttpStatusCode.NotFound);
        }

        if (access.AccessRole == FamilyAccessRole.Owner)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0003, "Cannot remove family owner."),
                HttpStatusCode.BadRequest);
        }

        access.IsActive = false;
        await _unitOfWork.CompleteAsync();
    }
}

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
    private readonly IEmailService _emailService;
    private readonly ILogger<FamilyInvitationService> _logger;

    public FamilyInvitationService(
        IUnitOfWork unitOfWork,
        IFamilyAuthorizationService familyAuthorizationService,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<FamilyInvitationService> logger)
    {
        _unitOfWork = unitOfWork;
        _familyAuthorizationService = familyAuthorizationService;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _logger = logger;
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

        // Send invitation email (best-effort — failure is logged but does not
        // roll back the already-persisted invitation record).
        try
        {
            User? inviter = await _unitOfWork.Users.GetAsync(invitedByUserId.Value);
            string? inviterName = inviter != null
                ? $"{inviter.FirstName} {inviter.LastName}".Trim()
                : null;

            string roleName = invitation.AccessRole switch
            {
                FamilyAccessRole.Owner    => "Vlasnik",
                FamilyAccessRole.Editor   => "Urednik",
                FamilyAccessRole.ReadOnly => "Posmatrač",
                _                         => invitation.AccessRole.ToString()
            };

            await _emailService.SendInvitationEmailAsync(
                invitation.Email,
                family.Name,
                inviterName,
                roleName,
                invitation.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Invitation email could not be sent to {Email} for family {FamilyId}. " +
                "The invitation is saved in the database and can be resent manually.",
                invitation.Email, familyId);
        }
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

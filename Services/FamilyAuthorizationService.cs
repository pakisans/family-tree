using System.Net;
using FamilyTree.Constants;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;
using FamilyTree.Services.Core.Auth;

namespace FamilyTree.Services;

public class FamilyAuthorizationService : IFamilyAuthorizationService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFamilyAccessRepository _familyAccessRepository;
    private readonly IFamilyRepository _familyRepository;

    public FamilyAuthorizationService(
        ICurrentUserService currentUserService,
        IFamilyAccessRepository familyAccessRepository,
        IFamilyRepository familyRepository)
    {
        _currentUserService = currentUserService;
        _familyAccessRepository = familyAccessRepository;
        _familyRepository = familyRepository;
    }

    public async Task EnsureCanReadFamilyByIdAsync(long familyId)
    {
        Family? family = await _familyRepository.GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        await EnsureCanReadFamilyAsync(family);
    }

    public async Task EnsureCanEditFamilyByIdAsync(long familyId)
    {
        Family? family = await _familyRepository.GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        await EnsureCanEditFamilyAsync(family);
    }

    public async Task EnsureCanManageFamilyAccessByIdAsync(long familyId)
    {
        Family? family = await _familyRepository.GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        await EnsureCanManageFamilyAccessAsync(family);
    }

    public async Task EnsureCanReadFamilyAsync(Family family)
    {
        if (_currentUserService.IsInRole(GlobalRoleName.SuperAdmin))
        {
            return;
        }

        if (family.IsPublic)
        {
            return;
        }

        long? currentUserId = _currentUserService.GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0006, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0006)),
                HttpStatusCode.Unauthorized);
        }

        FamilyAccess? familyAccess =
            await _familyAccessRepository.GetActiveAccessAsync(family.Id, currentUserId.Value);

        if (familyAccess == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0005, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0005)),
                HttpStatusCode.Forbidden);
        }
    }

    public async Task EnsureCanEditFamilyAsync(Family family)
    {
        if (_currentUserService.IsInRole(GlobalRoleName.SuperAdmin))
        {
            return;
        }

        long? currentUserId = _currentUserService.GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0006, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0006)),
                HttpStatusCode.Unauthorized);
        }

        FamilyAccess? familyAccess =
            await _familyAccessRepository.GetActiveAccessAsync(family.Id, currentUserId.Value);

        if (familyAccess == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0005, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0005)),
                HttpStatusCode.Forbidden);
        }

        if (familyAccess.AccessRole != FamilyAccessRole.Owner &&
            familyAccess.AccessRole != FamilyAccessRole.Editor)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0005, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0005)),
                HttpStatusCode.Forbidden);
        }
    }

    public async Task EnsureCanManageFamilyAccessAsync(Family family)
    {
        if (_currentUserService.IsInRole(GlobalRoleName.SuperAdmin))
        {
            return;
        }

        long? currentUserId = _currentUserService.GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0006, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0006)),
                HttpStatusCode.Unauthorized);
        }

        FamilyAccess? familyAccess =
            await _familyAccessRepository.GetActiveAccessAsync(family.Id, currentUserId.Value);

        if (familyAccess == null || familyAccess.AccessRole != FamilyAccessRole.Owner)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0005, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0005)),
                HttpStatusCode.Forbidden);
        }
    }

    public async Task<FamilyAccessRole?> GetFamilyAccessRoleAsync(long familyId, long userId)
    {
        FamilyAccess? familyAccess = await _familyAccessRepository.GetActiveAccessAsync(familyId, userId);
        return familyAccess?.AccessRole;
    }
}

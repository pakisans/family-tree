using System.Net;
using FamilyTree.Constants;
using FamilyTree.Dto;
using FamilyTree.Dto.Family;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;
using FamilyTree.Services.Core.Auth;

namespace FamilyTree.Services;

public class FamilyService : BaseService<Family, FamilyFilterRequest>, IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFamilyAuthorizationService _familyAuthorizationService;
    private readonly ICurrentUserService _currentUserService;

    public FamilyService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IFamilyRepository familyRepository,
        IPersonRepository personRepository,
        IFamilyAuthorizationService familyAuthorizationService,
        ICurrentUserService currentUserService)
        : base(unitOfWork, httpContextAccessor, familyRepository)
    {
        _familyRepository = familyRepository;
        _personRepository = personRepository;
        _familyAuthorizationService = familyAuthorizationService;
        _currentUserService = currentUserService;
    }

    protected override string[] SearchableProperties =>
    [
        nameof(Family.Name),
        nameof(Family.Slug),
        nameof(Family.OriginPlace),
        nameof(Family.SeoTitle)
    ];

    protected override async Task ValidateAsync(Family entity, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        if (string.IsNullOrWhiteSpace(entity.Slug))
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        string normalizedSlug = entity.Slug.Trim().ToLower();

        Family? existingFamily = await _familyRepository.GetAsync(
            family => family.Slug.ToLower() == normalizedSlug && family.Id != entity.Id);

        if (existingFamily != null)
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0002);
        }

        entity.Slug = normalizedSlug;
    }

    public override async Task<Family?> GetAsync(long id)
    {
        Family? family = await _familyRepository.GetAsync(id);

        if (family == null)
        {
            return null;
        }

        await _familyAuthorizationService.EnsureCanReadFamilyAsync(family);

        return family;
    }

    public override async Task<FilterList<Family>> GetListAsync(FamilyFilterRequest filterRequest)
    {
        FilterList<Family> result = await base.GetListAsync(filterRequest);

        List<Family> allowedFamilies = new List<Family>();

        foreach (Family family in result.Items)
        {
            bool canRead = await CanReadFamilyAsync(family);

            if (canRead)
            {
                allowedFamilies.Add(family);
            }
        }

        result.Items = allowedFamilies;
        result.TotalCount = allowedFamilies.Count;

        return result;
    }

    public async Task<FilterList<PersonSummaryDto>> GetMembersAsync(long familyId, PersonFilterRequest filterRequest)
    {
        Family family = await GetRequiredFamilyAsync(familyId);

        await _familyAuthorizationService.EnsureCanReadFamilyAsync(family);

        filterRequest.FamilyId = familyId;

        return await _personRepository.GetFamilyMembersPagedAsync(familyId, filterRequest);
    }

    public async Task<PersonDto?> GetMemberAsync(long familyId, long personId)
    {
        Family family = await GetRequiredFamilyAsync(familyId);

        await _familyAuthorizationService.EnsureCanReadFamilyAsync(family);

        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            return null;
        }

        if (person.FamilyId != familyId)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0003, "Person does not belong to the specified family."),
                HttpStatusCode.BadRequest);
        }

        return MapPersonToDto(person);
    }

    public async Task<PersonDto?> AddMemberAsync(long familyId, FamilyMemberRequestDto request)
    {
        Family family = await GetRequiredFamilyAsync(familyId);

        await _familyAuthorizationService.EnsureCanEditFamilyAsync(family);

        Person person = new Person
        {
            FamilyId = familyId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDate = request.BirthDate,
            DeathDate = request.DeathDate,
            Gender = request.Gender,
            BirthPlace = request.BirthPlace,
            DeathPlace = request.DeathPlace,
            Biography = request.Biography,
            IsPublic = request.IsPublic
        };

        Person? createdPerson = await UnitOfWork.GetRepository<Person>().AddAsync(person);

        if (createdPerson == null)
        {
            return null;
        }

        return MapPersonToDto(createdPerson);
    }

    public async Task<PersonDto?> UpdateMemberAsync(long familyId, long personId, FamilyMemberRequestDto request)
    {
        Family family = await GetRequiredFamilyAsync(familyId);

        await _familyAuthorizationService.EnsureCanEditFamilyAsync(family);

        Person? existingPerson = await _personRepository.GetAsync(personId);

        if (existingPerson == null)
        {
            return null;
        }

        if (existingPerson.FamilyId != familyId)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0003, "Person does not belong to the specified family."),
                HttpStatusCode.BadRequest);
        }

        existingPerson.FirstName = request.FirstName;
        existingPerson.LastName = request.LastName;
        existingPerson.BirthDate = request.BirthDate;
        existingPerson.DeathDate = request.DeathDate;
        existingPerson.Gender = request.Gender;
        existingPerson.BirthPlace = request.BirthPlace;
        existingPerson.DeathPlace = request.DeathPlace;
        existingPerson.Biography = request.Biography;
        existingPerson.IsPublic = request.IsPublic;

        Person? updatedPerson = await UnitOfWork
            .GetRepository<Person>()
            .UpdateAsync(existingPerson.Id, existingPerson);

        if (updatedPerson == null)
        {
            return null;
        }

        return MapPersonToDto(updatedPerson);
    }

    public override async Task<Family?> AddAsync(Family entity)
    {
        long? currentUserId = _currentUserService.GetCurrentUserId();

        if (!currentUserId.HasValue && !_currentUserService.IsInRole(GlobalRoleName.SuperAdmin))
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0006, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0006)),
                HttpStatusCode.Unauthorized);
        }

        if (currentUserId.HasValue && !entity.OwnerId.HasValue)
        {
            entity.OwnerId = currentUserId.Value;
        }

        Family? createdFamily = await base.AddAsync(entity);

        if (createdFamily == null)
        {
            return null;
        }

        if (createdFamily.OwnerId.HasValue)
        {
            FamilyAccess? existingAccess =
                await UnitOfWork.FamilyAccesses.GetActiveAccessAsync(createdFamily.Id, createdFamily.OwnerId.Value);

            if (existingAccess == null)
            {
                FamilyAccess ownerAccess = new FamilyAccess
                {
                    FamilyId = createdFamily.Id,
                    UserId = createdFamily.OwnerId.Value,
                    AccessRole = FamilyAccessRole.Owner,
                    InvitedByUserId = createdFamily.OwnerId.Value,
                    AcceptedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await UnitOfWork.FamilyAccesses.AddAsync(ownerAccess);
            }
        }

        return createdFamily;
    }

    public override async Task<Family?> UpdateAsync(long id, Family entity)
    {
        Family? existingFamily = await _familyRepository.GetAsync(id);

        if (existingFamily == null)
        {
            return null;
        }

        await _familyAuthorizationService.EnsureCanEditFamilyAsync(existingFamily);

        entity.Id = id;
        entity.OwnerId = existingFamily.OwnerId;

        return await base.UpdateAsync(id, entity);
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        Family? family = await _familyRepository.GetAsync(id);

        if (family == null)
        {
            return false;
        }

        await _familyAuthorizationService.EnsureCanEditFamilyAsync(family);

        return await base.DeleteAsync(id);
    }

    public override async Task<bool> ArchiveAsync(long id)
    {
        Family family = await GetRequiredFamilyAsync(id);

        await _familyAuthorizationService.EnsureCanEditFamilyAsync(family);

        return await base.ArchiveAsync(id);
    }

    public override async Task<bool> UnarchiveAsync(long id)
    {
        Family family = await GetRequiredFamilyAsync(id);

        await _familyAuthorizationService.EnsureCanEditFamilyAsync(family);

        return await base.UnarchiveAsync(id);
    }

    private async Task<Family> GetRequiredFamilyAsync(long familyId)
    {
        Family? family = await _familyRepository.GetAsync(familyId);

        if (family == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        return family;
    }

    private async Task<bool> CanReadFamilyAsync(Family family)
    {
        try
        {
            await _familyAuthorizationService.EnsureCanReadFamilyAsync(family);
            return true;
        }
        catch (HttpResponseException)
        {
            return false;
        }
    }

    private static PersonDto MapPersonToDto(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            DateCreated = person.DateCreated,
            OwnerId = person.OwnerId,
            ItemOrder = person.ItemOrder,
            Archived = person.Archived,
            FirstName = person.FirstName,
            LastName = person.LastName,
            BirthDate = person.BirthDate,
            DeathDate = person.DeathDate,
            Gender = person.Gender,
            BirthPlace = person.BirthPlace,
            DeathPlace = person.DeathPlace,
            Biography = person.Biography,
            IsPublic = person.IsPublic,
            FamilyId = person.FamilyId
        };
    }
}

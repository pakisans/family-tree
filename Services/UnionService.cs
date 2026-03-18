using System.Net;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class UnionService : BaseService<Union, UnionFilterRequest>, IUnionService
{
    private readonly IUnionRepository _unionRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFamilyAuthorizationService _familyAuthorizationService;

    public UnionService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IUnionRepository unionRepository,
        IPersonRepository personRepository,
        IFamilyAuthorizationService familyAuthorizationService)
        : base(unitOfWork, httpContextAccessor, unionRepository)
    {
        _unionRepository = unionRepository;
        _personRepository = personRepository;
        _familyAuthorizationService = familyAuthorizationService;
    }

    protected override string[] SearchableProperties => Array.Empty<string>();

    protected override async Task ValidateAsync(Union entity, bool isCreate)
    {
        if (entity.Person1Id == entity.Person2Id)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    UnionErrorCode.UNION_0001,
                    UnionErrorCode.GetDescription(UnionErrorCode.UNION_0001)),
                HttpStatusCode.BadRequest);
        }

        Person person1 = await GetRequiredPersonAsync(entity.Person1Id, UnionErrorCode.UNION_0002);
        Person person2 = await GetRequiredPersonAsync(entity.Person2Id, UnionErrorCode.UNION_0003);

        await EnsureEditAccessForPersonAsync(person1);
        await EnsureEditAccessForPersonAsync(person2);

        if (entity.StartDate.HasValue && entity.EndDate.HasValue && entity.EndDate < entity.StartDate)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    UnionErrorCode.UNION_0005,
                    UnionErrorCode.GetDescription(UnionErrorCode.UNION_0005)),
                HttpStatusCode.BadRequest);
        }

        long lowerPersonId = Math.Min(entity.Person1Id, entity.Person2Id);
        long higherPersonId = Math.Max(entity.Person1Id, entity.Person2Id);

        Union? existingUnion = await _unionRepository.GetAsync(union =>
            (
                (union.Person1Id == lowerPersonId && union.Person2Id == higherPersonId) ||
                (union.Person1Id == higherPersonId && union.Person2Id == lowerPersonId)
            ) &&
            union.Id != entity.Id &&
            union.IsActive &&
            entity.IsActive);

        if (existingUnion != null)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    UnionErrorCode.UNION_0006,
                    UnionErrorCode.GetDescription(UnionErrorCode.UNION_0006)),
                HttpStatusCode.Conflict);
        }

        entity.Person1Id = lowerPersonId;
        entity.Person2Id = higherPersonId;
    }

    public override async Task<Union?> GetAsync(long id)
    {
        Union? union = await _unionRepository.GetAsync(id);

        if (union == null)
        {
            return null;
        }

        Person person1 = await GetRequiredPersonAsync(union.Person1Id, UnionErrorCode.UNION_0002);
        Person person2 = await GetRequiredPersonAsync(union.Person2Id, UnionErrorCode.UNION_0003);

        await EnsureReadAccessForPersonAsync(person1);
        await EnsureReadAccessForPersonAsync(person2);

        return union;
    }

    public override async Task<Union?> UpdateAsync(long id, Union entity)
    {
        Union? existingUnion = await _unionRepository.GetAsync(id);

        if (existingUnion == null)
        {
            return null;
        }

        Person existingPerson1 = await GetRequiredPersonAsync(existingUnion.Person1Id, UnionErrorCode.UNION_0002);
        Person existingPerson2 = await GetRequiredPersonAsync(existingUnion.Person2Id, UnionErrorCode.UNION_0003);

        await EnsureEditAccessForPersonAsync(existingPerson1);
        await EnsureEditAccessForPersonAsync(existingPerson2);

        return await base.UpdateAsync(id, entity);
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        Union? existingUnion = await _unionRepository.GetAsync(id);

        if (existingUnion == null)
        {
            return false;
        }

        Person person1 = await GetRequiredPersonAsync(existingUnion.Person1Id, UnionErrorCode.UNION_0002);
        Person person2 = await GetRequiredPersonAsync(existingUnion.Person2Id, UnionErrorCode.UNION_0003);

        await EnsureEditAccessForPersonAsync(person1);
        await EnsureEditAccessForPersonAsync(person2);

        return await base.DeleteAsync(id);
    }

    public async Task<IList<UnionSummaryDto>> GetPartnersAsync(long personId)
    {
        Person person = await GetRequiredPersonAsync(personId, UnionErrorCode.UNION_0002);

        await EnsureReadAccessForPersonAsync(person);

        return await _unionRepository.GetPartnersAsync(personId);
    }

    public override async Task<FilterList<Union>> GetListAsync(UnionFilterRequest filterRequest)
    {
        FilterList<Union> result = await base.GetListAsync(filterRequest);

        if (filterRequest.PersonId.HasValue)
        {
            Person person = await GetRequiredPersonAsync(filterRequest.PersonId.Value, UnionErrorCode.UNION_0002);

            await EnsureReadAccessForPersonAsync(person);

            result.Items = result.Items
                .Where(x => x.Person1Id == filterRequest.PersonId.Value || x.Person2Id == filterRequest.PersonId.Value)
                .ToList();

            result.TotalCount = result.Items.Count;

            return result;
        }

        List<Union> allowedItems = new List<Union>();

        foreach (Union union in result.Items)
        {
            Person person1 = await GetRequiredPersonAsync(union.Person1Id, UnionErrorCode.UNION_0002);
            Person person2 = await GetRequiredPersonAsync(union.Person2Id, UnionErrorCode.UNION_0003);

            bool canRead = await CanReadUnionAsync(person1, person2);

            if (canRead)
            {
                allowedItems.Add(union);
            }
        }

        result.Items = allowedItems;
        result.TotalCount = allowedItems.Count;

        return result;
    }

    private async Task<Person> GetRequiredPersonAsync(long personId, string errorCode)
    {
        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(errorCode, UnionErrorCode.GetDescription(errorCode)),
                HttpStatusCode.NotFound);
        }

        return person;
    }

    private async Task EnsureReadAccessForPersonAsync(Person person)
    {
        if (person.FamilyId.HasValue)
        {
            await _familyAuthorizationService.EnsureCanReadFamilyByIdAsync(person.FamilyId.Value);
        }
    }

    private async Task EnsureEditAccessForPersonAsync(Person person)
    {
        if (person.FamilyId.HasValue)
        {
            await _familyAuthorizationService.EnsureCanEditFamilyByIdAsync(person.FamilyId.Value);
        }
    }

    private async Task<bool> CanReadUnionAsync(Person person1, Person person2)
    {
        try
        {
            await EnsureReadAccessForPersonAsync(person1);
            await EnsureReadAccessForPersonAsync(person2);
            return true;
        }
        catch (HttpResponseException)
        {
            return false;
        }
    }
}

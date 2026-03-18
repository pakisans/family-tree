using System.Net;
using FamilyTree.Constants;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class RelationshipService : BaseService<Relationship, RelationshipFilterRequest>, IRelationshipService
{
    private readonly IRelationshipRepository _relationshipRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFamilyAuthorizationService _familyAuthorizationService;

    public RelationshipService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IRelationshipRepository relationshipRepository,
        IPersonRepository personRepository,
        IFamilyAuthorizationService familyAuthorizationService)
        : base(unitOfWork, httpContextAccessor, relationshipRepository)
    {
        _relationshipRepository = relationshipRepository;
        _personRepository = personRepository;
        _familyAuthorizationService = familyAuthorizationService;
    }

    protected override string[] SearchableProperties => Array.Empty<string>();

    protected override async Task ValidateAsync(Relationship entity, bool isCreate)
    {
        if (entity.RelationshipType == RelationshipType.Unknown)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    RelationshipErrorCode.RELATIONSHIP_0001,
                    RelationshipErrorCode.GetDescription(RelationshipErrorCode.RELATIONSHIP_0001)),
                HttpStatusCode.BadRequest);
        }

        if (entity.FromPersonId == entity.ToPersonId)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    RelationshipErrorCode.RELATIONSHIP_0002,
                    RelationshipErrorCode.GetDescription(RelationshipErrorCode.RELATIONSHIP_0002)),
                HttpStatusCode.BadRequest);
        }

        Person fromPerson = await GetRequiredPersonAsync(
            entity.FromPersonId,
            RelationshipErrorCode.RELATIONSHIP_0003);

        Person toPerson = await GetRequiredPersonAsync(
            entity.ToPersonId,
            RelationshipErrorCode.RELATIONSHIP_0004);

        await EnsureEditAccessForPersonAsync(fromPerson);
        await EnsureEditAccessForPersonAsync(toPerson);

        Relationship? existingRelationship = await _relationshipRepository.GetAsync(
            relationship =>
                relationship.FromPersonId == entity.FromPersonId &&
                relationship.ToPersonId == entity.ToPersonId &&
                relationship.RelationshipType == entity.RelationshipType &&
                relationship.Id != entity.Id);

        if (existingRelationship != null)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    RelationshipErrorCode.RELATIONSHIP_0005,
                    RelationshipErrorCode.GetDescription(RelationshipErrorCode.RELATIONSHIP_0005)),
                HttpStatusCode.Conflict);
        }
    }

    public override async Task<Relationship?> GetAsync(long id)
    {
        Relationship? relationship = await _relationshipRepository.GetAsync(id);

        if (relationship == null)
        {
            return null;
        }

        Person fromPerson = await GetRequiredPersonAsync(
            relationship.FromPersonId,
            RelationshipErrorCode.RELATIONSHIP_0003);

        Person toPerson = await GetRequiredPersonAsync(
            relationship.ToPersonId,
            RelationshipErrorCode.RELATIONSHIP_0004);

        await EnsureReadAccessForPersonAsync(fromPerson);
        await EnsureReadAccessForPersonAsync(toPerson);

        return relationship;
    }

    public override async Task<Relationship?> AddAsync(Relationship entity)
    {
        return await base.AddAsync(entity);
    }

    public override async Task<Relationship?> UpdateAsync(long id, Relationship entity)
    {
        Relationship? existingRelationship = await _relationshipRepository.GetAsync(id);

        if (existingRelationship == null)
        {
            return null;
        }

        Person existingFromPerson = await GetRequiredPersonAsync(
            existingRelationship.FromPersonId,
            RelationshipErrorCode.RELATIONSHIP_0003);

        Person existingToPerson = await GetRequiredPersonAsync(
            existingRelationship.ToPersonId,
            RelationshipErrorCode.RELATIONSHIP_0004);

        await EnsureEditAccessForPersonAsync(existingFromPerson);
        await EnsureEditAccessForPersonAsync(existingToPerson);

        return await base.UpdateAsync(id, entity);
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        Relationship? existingRelationship = await _relationshipRepository.GetAsync(id);

        if (existingRelationship == null)
        {
            return false;
        }

        Person fromPerson = await GetRequiredPersonAsync(
            existingRelationship.FromPersonId,
            RelationshipErrorCode.RELATIONSHIP_0003);

        Person toPerson = await GetRequiredPersonAsync(
            existingRelationship.ToPersonId,
            RelationshipErrorCode.RELATIONSHIP_0004);

        await EnsureEditAccessForPersonAsync(fromPerson);
        await EnsureEditAccessForPersonAsync(toPerson);

        return await base.DeleteAsync(id);
    }

    public override async Task<FilterList<Relationship>> GetListAsync(RelationshipFilterRequest filterRequest)
    {
        FilterList<Relationship> result = await base.GetListAsync(filterRequest);

        if (filterRequest.FromPersonId.HasValue)
        {
            Person fromPerson = await GetRequiredPersonAsync(
                filterRequest.FromPersonId.Value,
                RelationshipErrorCode.RELATIONSHIP_0003);

            await EnsureReadAccessForPersonAsync(fromPerson);

            result.Items = result.Items
                .Where(x => x.FromPersonId == filterRequest.FromPersonId.Value)
                .ToList();
        }

        if (filterRequest.ToPersonId.HasValue)
        {
            Person toPerson = await GetRequiredPersonAsync(
                filterRequest.ToPersonId.Value,
                RelationshipErrorCode.RELATIONSHIP_0004);

            await EnsureReadAccessForPersonAsync(toPerson);

            result.Items = result.Items
                .Where(x => x.ToPersonId == filterRequest.ToPersonId.Value)
                .ToList();
        }

        if (!filterRequest.FromPersonId.HasValue && !filterRequest.ToPersonId.HasValue)
        {
            List<Relationship> allowedItems = new List<Relationship>();

            foreach (Relationship relationship in result.Items)
            {
                Person fromPerson = await GetRequiredPersonAsync(
                    relationship.FromPersonId,
                    RelationshipErrorCode.RELATIONSHIP_0003);

                Person toPerson = await GetRequiredPersonAsync(
                    relationship.ToPersonId,
                    RelationshipErrorCode.RELATIONSHIP_0004);

                bool canRead = await CanReadRelationshipAsync(fromPerson, toPerson);

                if (canRead)
                {
                    allowedItems.Add(relationship);
                }
            }

            result.Items = allowedItems;
        }

        result.TotalCount = result.Items.Count;

        return result;
    }

    private async Task<Person> GetRequiredPersonAsync(long personId, string errorCode)
    {
        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(errorCode, RelationshipErrorCode.GetDescription(errorCode)),
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

    private async Task<bool> CanReadRelationshipAsync(Person fromPerson, Person toPerson)
    {
        try
        {
            await EnsureReadAccessForPersonAsync(fromPerson);
            await EnsureReadAccessForPersonAsync(toPerson);
            return true;
        }
        catch (HttpResponseException)
        {
            return false;
        }
    }
}

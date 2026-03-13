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

    public RelationshipService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IRelationshipRepository relationshipRepository,
        IPersonRepository personRepository)
        : base(unitOfWork, httpContextAccessor, relationshipRepository)
    {
        _relationshipRepository = relationshipRepository;
        _personRepository = personRepository;
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

        Person? fromPerson = await _personRepository.GetAsync(entity.FromPersonId);

        if (fromPerson == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    RelationshipErrorCode.RELATIONSHIP_0003,
                    RelationshipErrorCode.GetDescription(RelationshipErrorCode.RELATIONSHIP_0003)),
                HttpStatusCode.NotFound);
        }

        Person? toPerson = await _personRepository.GetAsync(entity.ToPersonId);

        if (toPerson == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    RelationshipErrorCode.RELATIONSHIP_0004,
                    RelationshipErrorCode.GetDescription(RelationshipErrorCode.RELATIONSHIP_0004)),
                HttpStatusCode.NotFound);
        }

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

    public override async Task<FilterList<Relationship>> GetListAsync(RelationshipFilterRequest filterRequest)
    {
        FilterList<Relationship> result = await base.GetListAsync(filterRequest);

        if (filterRequest.FromPersonId.HasValue)
        {
            result.Items = result.Items
                .Where(x => x.FromPersonId == filterRequest.FromPersonId.Value)
                .ToList();
        }

        if (filterRequest.ToPersonId.HasValue)
        {
            result.Items = result.Items
                .Where(x => x.ToPersonId == filterRequest.ToPersonId.Value)
                .ToList();
        }

        result.TotalCount = result.Items.Count;

        return result;
    }
}

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

    public UnionService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IUnionRepository unionRepository,
        IPersonRepository personRepository)
        : base(unitOfWork, httpContextAccessor, unionRepository)
    {
        _unionRepository = unionRepository;
        _personRepository = personRepository;
    }

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

        Person? person1 = await _personRepository.GetAsync(entity.Person1Id);

        if (person1 == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    UnionErrorCode.UNION_0002,
                    UnionErrorCode.GetDescription(UnionErrorCode.UNION_0002)),
                HttpStatusCode.NotFound);
        }

        Person? person2 = await _personRepository.GetAsync(entity.Person2Id);

        if (person2 == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(
                    UnionErrorCode.UNION_0003,
                    UnionErrorCode.GetDescription(UnionErrorCode.UNION_0003)),
                HttpStatusCode.NotFound);
        }

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

    public async Task<IList<UnionSummaryDto>> GetPartnersAsync(long personId)
    {
        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        return await _unionRepository.GetPartnersAsync(personId);
    }

    public override async Task<FilterList<Union>> GetListAsync(UnionFilterRequest filterRequest)
    {
        FilterList<Union> result = await base.GetListAsync(filterRequest);

        if (filterRequest.PersonId.HasValue)
        {
            result.Items = result.Items
                .Where(x => x.Person1Id == filterRequest.PersonId.Value || x.Person2Id == filterRequest.PersonId.Value)
                .ToList();

            result.TotalCount = result.Items.Count;
        }

        return result;
    }
}

using System.Net;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class PersonService : BaseService<Person, PersonFilterRequest>, IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IRelationshipRepository _relationshipRepository;

    public PersonService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IPersonRepository personRepository,
        IRelationshipRepository relationshipRepository)
        : base(unitOfWork, httpContextAccessor, personRepository)
    {
        _personRepository = personRepository;
        _relationshipRepository = relationshipRepository;
    }

    protected override string[] SearchableProperties =>
    [
        nameof(Person.FirstName),
        nameof(Person.LastName),
        nameof(Person.BirthPlace),
        nameof(Person.DeathPlace)
    ];

    protected override Task ValidateAsync(Person entity, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(entity.FirstName))
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        if (string.IsNullOrWhiteSpace(entity.LastName))
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        if (entity.BirthDate.HasValue && entity.DeathDate.HasValue && entity.DeathDate < entity.BirthDate)
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        return Task.CompletedTask;
    }

    public async Task<IList<PersonSummaryDto>> GetParentsAsync(long personId)
    {
        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        return await _relationshipRepository.GetParentsAsync(personId);
    }

    public async Task<IList<PersonSummaryDto>> GetChildrenAsync(long personId)
    {
        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        return await _relationshipRepository.GetChildrenAsync(personId);
    }
}

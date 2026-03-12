using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class PersonService : BaseService<Person, PersonFilterRequest>, IPersonService
{
    public PersonService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IPersonRepository personRepository)
        : base(unitOfWork, httpContextAccessor, personRepository)
    {
    }

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
}

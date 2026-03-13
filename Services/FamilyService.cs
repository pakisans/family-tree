using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class FamilyService : BaseService<Family, FamilyFilterRequest>, IFamilyService
{
    private readonly IFamilyRepository _familyRepository;

    public FamilyService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IFamilyRepository familyRepository)
        : base(unitOfWork, httpContextAccessor, familyRepository)
    {
        _familyRepository = familyRepository;
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
}

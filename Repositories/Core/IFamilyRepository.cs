using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;

namespace FamilyTree.Repositories.Core;

public interface IFamilyRepository : IBaseRepository<Family>
{
    public Task<FilterList<Family>> GetPublicFamiliesPagedAsync(FamilyFilterRequest filterRequest);

    public Task<Family?> GetPublicBySlugAsync(string slug);
}

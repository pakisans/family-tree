using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IUnionRepository : IBaseRepository<Union>
{
    public Task<IList<UnionSummaryDto>> GetPartnersAsync(long personId);
}

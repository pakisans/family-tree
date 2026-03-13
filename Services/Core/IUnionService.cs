using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;

namespace FamilyTree.Services.Core;

public interface IUnionService : IBaseService<Union, UnionFilterRequest>
{
    public Task<IList<UnionSummaryDto>> GetPartnersAsync(long personId);
}

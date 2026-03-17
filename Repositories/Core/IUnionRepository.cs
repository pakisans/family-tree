using FamilyTree.Dto;
using FamilyTree.Dto.Response.Graph;
using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IUnionRepository : IBaseRepository<Union>
{
    public Task<IList<UnionSummaryDto>> GetPartnersAsync(long personId);
    public Task<IList<PersonSummaryDto>> GetPartnerPersonsAsync(long personId);
    public Task<IList<PersonRelationRecordDto>> GetUnionRelationsAsync(ICollection<long> personIds);
}

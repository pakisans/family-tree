using FamilyTree.Dto;
using FamilyTree.Dto.Response.Graph;
using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IRelationshipRepository : IBaseRepository<Relationship>
{
    public Task<IList<PersonSummaryDto>> GetParentsAsync(long personId);
    public Task<IList<PersonSummaryDto>> GetChildrenAsync(long personId);

    public Task<IList<PersonRelationRecordDto>> GetParentRelationsForChildrenAsync(ICollection<long> childIds);
    public Task<IList<PersonRelationRecordDto>> GetChildRelationsForParentsAsync(ICollection<long> parentIds);
}

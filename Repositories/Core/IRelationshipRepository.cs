using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IRelationshipRepository : IBaseRepository<Relationship>
{
    public Task<IList<PersonSummaryDto>> GetParentsAsync(long personId);
    public Task<IList<PersonSummaryDto>> GetChildrenAsync(long personId);
}

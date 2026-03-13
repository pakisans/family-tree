using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IPersonRepository : IBaseRepository<Person>
{
    public Task<IList<PersonSummaryDto>> GetFamilyMembersAsync(long familyId);
    public Task<PersonSummaryDto?> GetSummaryAsync(long personId);
}

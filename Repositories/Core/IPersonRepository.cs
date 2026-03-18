using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;

namespace FamilyTree.Repositories.Core;

public interface IPersonRepository : IBaseRepository<Person>
{
    public Task<IList<PersonSummaryDto>> GetFamilyMembersAsync(long familyId);
    public Task<FilterList<PersonSummaryDto>> GetFamilyMembersPagedAsync(long familyId, PersonFilterRequest filterRequest);
    public Task<FilterList<PersonSummaryDto>> GetPublicFamilyMembersPagedAsync(long familyId, PersonFilterRequest filterRequest);
    public Task<PersonSummaryDto?> GetSummaryAsync(long personId);
    public Task<IList<PersonSummaryDto>> GetSummariesByIdsAsync(ICollection<long> personIds);
}

using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;

namespace FamilyTree.Services.Core;

public interface IFamilyService : IBaseService<Family, FamilyFilterRequest>
{
    public Task<IList<PersonSummaryDto>> GetMembersAsync(long familyId);
}

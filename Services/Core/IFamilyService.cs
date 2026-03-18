using FamilyTree.Dto;
using FamilyTree.Dto.Family;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;

namespace FamilyTree.Services.Core;

public interface IFamilyService : IBaseService<Family, FamilyFilterRequest>
{
    public Task<FilterList<PersonSummaryDto>> GetMembersAsync(long familyId, PersonFilterRequest filterRequest);
    public Task<PersonDto?> GetMemberAsync(long familyId, long personId);
    public Task<PersonDto?> AddMemberAsync(long familyId, FamilyMemberRequestDto request);
    public Task<PersonDto?> UpdateMemberAsync(long familyId, long personId, FamilyMemberRequestDto request);
}

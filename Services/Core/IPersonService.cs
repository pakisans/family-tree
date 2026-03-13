using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;

namespace FamilyTree.Services.Core;

public interface IPersonService : IBaseService<Person, PersonFilterRequest>
{
    public Task<IList<PersonSummaryDto>> GetParentsAsync(long personId);
    public Task<IList<PersonSummaryDto>> GetChildrenAsync(long personId);

    public Task<PersonTreeDto> GetTreeAsync(long personId);
}

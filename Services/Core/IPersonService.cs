using FamilyTree.Entity;
using FamilyTree.Features.Filtering;

namespace FamilyTree.Services.Core;

public interface IPersonService : IBaseService<Person, PersonFilterRequest>
{
}

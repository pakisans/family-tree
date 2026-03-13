using FamilyTree.Entity;
using FamilyTree.Features.Filtering;

namespace FamilyTree.Services.Core;

public interface IRelationshipService : IBaseService<Relationship, RelationshipFilterRequest>
{
}

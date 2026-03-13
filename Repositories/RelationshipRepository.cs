using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;

namespace FamilyTree.Repositories;

public class RelationshipRepository : BaseRepository<Relationship>, IRelationshipRepository
{
    public RelationshipRepository(FamilyTreeContext context) : base(context)
    {
    }
}

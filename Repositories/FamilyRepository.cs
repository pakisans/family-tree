using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;

namespace FamilyTree.Repositories;

public class FamilyRepository : BaseRepository<Family>, IFamilyRepository
{
    public FamilyRepository(FamilyTreeContext context) : base(context)
    {
    }
}

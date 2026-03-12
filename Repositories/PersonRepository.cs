using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;

namespace FamilyTree.Repositories;

public class PersonRepository : BaseRepository<Person>, IPersonRepository
{
    public PersonRepository(FamilyTreeContext dbContext) : base(dbContext)
    {
    }
}

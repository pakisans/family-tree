using FamilyTree.Database;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class PersonRepository : BaseRepository<Person>, IPersonRepository
{
    public PersonRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<IList<PersonSummaryDto>> GetFamilyMembersAsync(long familyId)
    {
        return await DbContext.Persons
            .AsNoTracking()
            .Where(person => person.FamilyId == familyId)
            .OrderBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .Select(person => new PersonSummaryDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                BirthDate = person.BirthDate,
                DeathDate = person.DeathDate,
                Gender = person.Gender,
                FamilyId = person.FamilyId
            })
            .ToListAsync();
    }
}

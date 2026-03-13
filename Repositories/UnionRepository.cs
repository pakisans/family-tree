using FamilyTree.Database;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class UnionRepository : BaseRepository<Union>, IUnionRepository
{
    public UnionRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<IList<UnionSummaryDto>> GetPartnersAsync(long personId)
    {
        return await DbContext.Unions
            .AsNoTracking()
            .Where(union => union.Person1Id == personId || union.Person2Id == personId)
            .OrderByDescending(union => union.IsActive)
            .ThenByDescending(union => union.StartDate)
            .Select(union => new UnionSummaryDto
            {
                UnionId = union.Id,
                StartDate = union.StartDate,
                EndDate = union.EndDate,
                IsActive = union.IsActive,
                Notes = union.Notes,
                Partner = union.Person1Id == personId
                    ? new PersonSummaryDto
                    {
                        Id = union.Person2!.Id,
                        FirstName = union.Person2.FirstName,
                        LastName = union.Person2.LastName,
                        BirthDate = union.Person2.BirthDate,
                        DeathDate = union.Person2.DeathDate,
                        Gender = union.Person2.Gender,
                        FamilyId = union.Person2.FamilyId
                    }
                    : new PersonSummaryDto
                    {
                        Id = union.Person1!.Id,
                        FirstName = union.Person1.FirstName,
                        LastName = union.Person1.LastName,
                        BirthDate = union.Person1.BirthDate,
                        DeathDate = union.Person1.DeathDate,
                        Gender = union.Person1.Gender,
                        FamilyId = union.Person1.FamilyId
                    }
            })
            .ToListAsync();
    }
}

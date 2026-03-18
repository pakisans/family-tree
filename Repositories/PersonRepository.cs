using FamilyTree.Database;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
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
                FamilyId = person.FamilyId,
                BirthPlace = person.BirthPlace,
                IsPublic = person.IsPublic
            })
            .ToListAsync();
    }

    public async Task<FilterList<PersonSummaryDto>> GetFamilyMembersPagedAsync(long familyId, PersonFilterRequest filterRequest)
    {
        IQueryable<Person> query = DbContext.Persons
            .AsNoTracking()
            .Where(person => person.FamilyId == familyId);

        if (!string.IsNullOrWhiteSpace(filterRequest.Term))
        {
            string term = filterRequest.Term.ToLower();

            query = query.Where(person =>
                person.FirstName.ToLower().Contains(term) ||
                person.LastName.ToLower().Contains(term));
        }

        int totalCount = await query.CountAsync();

        List<PersonSummaryDto> items = await query
            .OrderBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .Skip(filterRequest.Page * filterRequest.PerPage)
            .Take(filterRequest.PerPage)
            .Select(person => new PersonSummaryDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                BirthDate = person.BirthDate,
                DeathDate = person.DeathDate,
                Gender = person.Gender,
                FamilyId = person.FamilyId,
                BirthPlace = person.BirthPlace,
                IsPublic = person.IsPublic
            })
            .ToListAsync();

        return new FilterList<PersonSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filterRequest.Page,
            PerPage = filterRequest.PerPage
        };
    }

    public async Task<FilterList<PersonSummaryDto>> GetPublicFamilyMembersPagedAsync(long familyId, PersonFilterRequest filterRequest)
    {
        IQueryable<Person> query = DbContext.Persons
            .AsNoTracking()
            .Where(person => person.FamilyId == familyId && person.IsPublic);

        if (!string.IsNullOrWhiteSpace(filterRequest.Term))
        {
            string term = filterRequest.Term.ToLower();

            query = query.Where(person =>
                person.FirstName.ToLower().Contains(term) ||
                person.LastName.ToLower().Contains(term));
        }

        int totalCount = await query.CountAsync();

        List<PersonSummaryDto> items = await query
            .OrderBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .Skip(filterRequest.Page * filterRequest.PerPage)
            .Take(filterRequest.PerPage)
            .Select(person => new PersonSummaryDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                BirthDate = person.BirthDate,
                DeathDate = person.DeathDate,
                Gender = person.Gender,
                FamilyId = person.FamilyId,
                BirthPlace = person.BirthPlace,
                IsPublic = person.IsPublic
            })
            .ToListAsync();

        return new FilterList<PersonSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filterRequest.Page,
            PerPage = filterRequest.PerPage
        };
    }

    public async Task<PersonSummaryDto?> GetSummaryAsync(long personId)
    {
        return await DbContext.Persons
            .AsNoTracking()
            .Where(person => person.Id == personId)
            .Select(person => new PersonSummaryDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                BirthDate = person.BirthDate,
                DeathDate = person.DeathDate,
                Gender = person.Gender,
                FamilyId = person.FamilyId,
                BirthPlace = person.BirthPlace,
                IsPublic = person.IsPublic
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IList<PersonSummaryDto>> GetSummariesByIdsAsync(ICollection<long> personIds)
    {
        if (personIds.Count == 0)
        {
            return new List<PersonSummaryDto>();
        }

        return await DbContext.Persons
            .AsNoTracking()
            .Where(person => personIds.Contains(person.Id))
            .Select(person => new PersonSummaryDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                BirthDate = person.BirthDate,
                DeathDate = person.DeathDate,
                Gender = person.Gender,
                FamilyId = person.FamilyId,
                BirthPlace = person.BirthPlace,
                IsPublic = person.IsPublic
            })
            .ToListAsync();
    }
}

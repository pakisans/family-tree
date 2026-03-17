using FamilyTree.Constants;
using FamilyTree.Database;
using FamilyTree.Dto;
using FamilyTree.Dto.Response.Graph;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class RelationshipRepository : BaseRepository<Relationship>, IRelationshipRepository
{
    public RelationshipRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<IList<PersonSummaryDto>> GetParentsAsync(long personId)
    {
        return await DbContext.Relationships
            .AsNoTracking()
            .Where(relationship =>
                relationship.ToPersonId == personId &&
                (relationship.RelationshipType == RelationshipType.Parent ||
                 relationship.RelationshipType == RelationshipType.AdoptiveParent))
            .Select(relationship => new PersonSummaryDto
            {
                Id = relationship.FromPerson!.Id,
                FirstName = relationship.FromPerson.FirstName,
                LastName = relationship.FromPerson.LastName,
                BirthDate = relationship.FromPerson.BirthDate,
                DeathDate = relationship.FromPerson.DeathDate,
                Gender = relationship.FromPerson.Gender,
                FamilyId = relationship.FromPerson.FamilyId
            })
            .OrderBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .ToListAsync();
    }

    public async Task<IList<PersonSummaryDto>> GetChildrenAsync(long personId)
    {
        return await DbContext.Relationships
            .AsNoTracking()
            .Where(relationship =>
                relationship.FromPersonId == personId &&
                (relationship.RelationshipType == RelationshipType.Parent ||
                 relationship.RelationshipType == RelationshipType.AdoptiveParent))
            .Select(relationship => new PersonSummaryDto
            {
                Id = relationship.ToPerson!.Id,
                FirstName = relationship.ToPerson.FirstName,
                LastName = relationship.ToPerson.LastName,
                BirthDate = relationship.ToPerson.BirthDate,
                DeathDate = relationship.ToPerson.DeathDate,
                Gender = relationship.ToPerson.Gender,
                FamilyId = relationship.ToPerson.FamilyId
            })
            .OrderBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .ToListAsync();
    }

    public async Task<IList<PersonRelationRecordDto>> GetParentRelationsForChildrenAsync(ICollection<long> childIds)
    {
        if (childIds.Count == 0)
        {
            return new List<PersonRelationRecordDto>();
        }

        return await DbContext.Relationships
            .AsNoTracking()
            .Where(relationship =>
                childIds.Contains(relationship.ToPersonId) &&
                (relationship.RelationshipType == RelationshipType.Parent ||
                 relationship.RelationshipType == RelationshipType.AdoptiveParent))
            .Select(relationship => new PersonRelationRecordDto
            {
                SourcePersonId = relationship.FromPersonId,
                TargetPersonId = relationship.ToPersonId,
                EdgeType = "parent-child"
            })
            .ToListAsync();
    }

    public async Task<IList<PersonRelationRecordDto>> GetChildRelationsForParentsAsync(ICollection<long> parentIds)
    {
        if (parentIds.Count == 0)
        {
            return new List<PersonRelationRecordDto>();
        }

        return await DbContext.Relationships
            .AsNoTracking()
            .Where(relationship =>
                parentIds.Contains(relationship.FromPersonId) &&
                (relationship.RelationshipType == RelationshipType.Parent ||
                 relationship.RelationshipType == RelationshipType.AdoptiveParent))
            .Select(relationship => new PersonRelationRecordDto
            {
                SourcePersonId = relationship.FromPersonId,
                TargetPersonId = relationship.ToPersonId,
                EdgeType = "parent-child"
            })
            .ToListAsync();
    }
}

using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Converters;

public class RelationshipConverter : BaseConverter<Relationship, RelationshipDto>, IRelationshipConverter
{
    public override RelationshipDto MapToDto(Relationship entity)
    {
        return new RelationshipDto
        {
            Id = entity.Id,
            DateCreated = entity.DateCreated,
            OwnerId = entity.OwnerId,
            ItemOrder = entity.ItemOrder,
            Archived = entity.Archived,
            FromPersonId = entity.FromPersonId,
            ToPersonId = entity.ToPersonId,
            RelationshipType = entity.RelationshipType,
            Notes = entity.Notes
        };
    }

    public override Relationship MapToEntity(RelationshipDto dto)
    {
        return new Relationship
        {
            Id = dto.Id,
            OwnerId = dto.OwnerId,
            ItemOrder = dto.ItemOrder,
            Archived = dto.Archived,
            FromPersonId = dto.FromPersonId,
            ToPersonId = dto.ToPersonId,
            RelationshipType = dto.RelationshipType,
            Notes = dto.Notes
        };
    }
}

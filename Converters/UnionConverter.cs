using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Converters;

public class UnionConverter : BaseConverter<Union, UnionDto>, IUnionConverter
{
    public override UnionDto MapToDto(Union entity)
    {
        return new UnionDto
        {
            Id = entity.Id,
            DateCreated = entity.DateCreated,
            OwnerId = entity.OwnerId,
            ItemOrder = entity.ItemOrder,
            Archived = entity.Archived,
            Person1Id = entity.Person1Id,
            Person2Id = entity.Person2Id,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }

    public override Union MapToEntity(UnionDto dto)
    {
        return new Union
        {
            Id = dto.Id,
            OwnerId = dto.OwnerId,
            ItemOrder = dto.ItemOrder,
            Archived = dto.Archived,
            Person1Id = dto.Person1Id,
            Person2Id = dto.Person2Id,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            Notes = dto.Notes
        };
    }
}

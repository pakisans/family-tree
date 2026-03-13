using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Converters;

public class FamilyConverter : BaseConverter<Family, FamilyDto>, IFamilyConverter
{
    public override FamilyDto MapToDto(Family entity)
    {
        return new FamilyDto
        {
            Id = entity.Id,
            DateCreated = entity.DateCreated,
            OwnerId = entity.OwnerId,
            ItemOrder = entity.ItemOrder,
            Archived = entity.Archived,
            Name = entity.Name,
            Slug = entity.Slug,
            Description = entity.Description,
            OriginPlace = entity.OriginPlace,
            IsPublic = entity.IsPublic,
            AllowPublicTree = entity.AllowPublicTree,
            SeoTitle = entity.SeoTitle,
            SeoDescription = entity.SeoDescription
        };
    }

    public override Family MapToEntity(FamilyDto dto)
    {
        return new Family
        {
            Id = dto.Id,
            OwnerId = dto.OwnerId,
            ItemOrder = dto.ItemOrder,
            Archived = dto.Archived,
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            OriginPlace = dto.OriginPlace,
            IsPublic = dto.IsPublic,
            AllowPublicTree = dto.AllowPublicTree,
            SeoTitle = dto.SeoTitle,
            SeoDescription = dto.SeoDescription
        };
    }
}

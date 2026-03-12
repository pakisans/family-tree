using FamilyTree.Converters.Core;

namespace FamilyTree.Converters;

public abstract class BaseConverter<TEntity, TEntityDto> : IBaseConverter<TEntity, TEntityDto>
    where TEntity : class
    where TEntityDto : class
{
    public abstract TEntityDto MapToDto(TEntity entity);

    public virtual IList<TEntityDto> MapToDtos(IList<TEntity> entities)
    {
        if (entities == null)
        {
            return new List<TEntityDto>();
        }

        return entities.Select(MapToDto).ToList();
    }

    public abstract TEntity MapToEntity(TEntityDto dto);
}

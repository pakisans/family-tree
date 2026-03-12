namespace FamilyTree.Converters.Core;

public interface IBaseConverter<TEntity, TEntityDto>
    where TEntity : class
    where TEntityDto : class
{
    public TEntityDto MapToDto(TEntity entity);

    public IList<TEntityDto> MapToDtos(IList<TEntity> entities);

    public TEntity MapToEntity(TEntityDto dto);
}

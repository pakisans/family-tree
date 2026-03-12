using FamilyTree.Converters.Core;
using FamilyTree.Dtos;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[ApiController]
public class BaseController<TEntity, TEntityDto, TFilter> : ControllerBase
    where TEntity : BaseEntity
    where TEntityDto : BaseDto
    where TFilter : BaseFilterRequest
{
    protected readonly IBaseService<TEntity, TFilter> BaseService;
    protected readonly IBaseConverter<TEntity, TEntityDto> BaseConverter;

    public BaseController(
        IBaseService<TEntity, TFilter> baseService,
        IBaseConverter<TEntity, TEntityDto> baseConverter)
    {
        BaseService = baseService;
        BaseConverter = baseConverter;
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateEntity([FromBody] TEntityDto entityDto)
    {
        TEntity entity = BaseConverter.MapToEntity(entityDto);
        TEntity? addedEntity = await BaseService.AddAsync(entity);

        if (addedEntity == null)
        {
            return BadRequest();
        }

        return Ok(BaseConverter.MapToDto(addedEntity));
    }

    [HttpPut("{id:long}")]
    public virtual async Task<IActionResult> UpdateEntity([FromRoute] long id, [FromBody] TEntityDto entityDto)
    {
        TEntity entity = BaseConverter.MapToEntity(entityDto);
        TEntity? updatedEntity = await BaseService.UpdateAsync(id, entity);

        if (updatedEntity == null)
        {
            return NotFound();
        }

        return Ok(BaseConverter.MapToDto(updatedEntity));
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetList([FromQuery] TFilter filterRequest)
    {
        FilterList<TEntity> resultList = await BaseService.GetListAsync(filterRequest);

        FilterList<TEntityDto> resultDtoList = new FilterList<TEntityDto>
        {
            Items = BaseConverter.MapToDtos(resultList.Items).ToList(),
            TotalCount = resultList.TotalCount,
            Page = resultList.Page,
            PerPage = resultList.PerPage
        };

        return Ok(resultDtoList);
    }

    [HttpGet("{id:long}")]
    public virtual async Task<IActionResult> GetById([FromRoute] long id)
    {
        TEntity? entity = await BaseService.GetAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(BaseConverter.MapToDto(entity));
    }

    [HttpGet("all")]
    public virtual async Task<IActionResult> GetListAll()
    {
        List<TEntity> entities = await BaseService.GetAllAsync();
        return Ok(BaseConverter.MapToDtos(entities));
    }

    [HttpDelete("{id:long}")]
    public virtual async Task<IActionResult> Delete([FromRoute] long id)
    {
        bool deleted = await BaseService.DeleteAsync(id);
        return Ok(deleted);
    }

    [HttpPatch("archive/{id:long}")]
    public virtual async Task<IActionResult> Archive([FromRoute] long id)
    {
        bool archived = await BaseService.ArchiveAsync(id);
        return Ok(archived);
    }

    [HttpPatch("unarchive/{id:long}")]
    public virtual async Task<IActionResult> Unarchive([FromRoute] long id)
    {
        bool unarchived = await BaseService.UnarchiveAsync(id);
        return Ok(unarchived);
    }
}

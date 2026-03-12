using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class BaseService<TEntity, TFilter> : IBaseService<TEntity, TFilter>
    where TEntity : BaseEntity
    where TFilter : BaseFilterRequest
{
    protected readonly IBaseRepository<TEntity> BaseRepository;

    public BaseService(IBaseRepository<TEntity> baseRepository)
    {
        BaseRepository = baseRepository;
    }

    protected virtual string[] SearchableProperties => Array.Empty<string>();

    public virtual async Task<TEntity?> GetAsync(long id)
    {
        return await BaseRepository.GetAsync(id);
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await BaseRepository.GetAllAsync();
    }

    public virtual async Task<FilterList<TEntity>> GetListAsync(TFilter filterRequest)
    {
        return await BaseRepository.GetListAsync(filterRequest, SearchableProperties);
    }

    public virtual async Task<TEntity?> AddAsync(TEntity entity)
    {
        await ValidateAsync(entity, true);
        return await BaseRepository.AddAsync(entity);
    }

    public virtual async Task<TEntity?> UpdateAsync(long id, TEntity entity)
    {
        await ValidateAsync(entity, false);
        return await BaseRepository.UpdateAsync(id, entity);
    }

    public virtual async Task<bool> DeleteAsync(long id)
    {
        return await BaseRepository.DeleteAsync(id);
    }

    public virtual async Task<bool> ArchiveAsync(long id)
    {
        return await BaseRepository.ArchiveAsync(id);
    }

    public virtual async Task<bool> UnarchiveAsync(long id)
    {
        return await BaseRepository.UnarchiveAsync(id);
    }

    protected virtual Task ValidateAsync(TEntity entity, bool isCreate)
    {
        return Task.CompletedTask;
    }
}

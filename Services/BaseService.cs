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
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IBaseRepository<TEntity> BaseRepository;
    protected readonly IHttpContextAccessor HttpContextAccessor;

    public BaseService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IBaseRepository<TEntity>? baseRepository = null)
    {
        UnitOfWork = unitOfWork;
        HttpContextAccessor = httpContextAccessor;
        BaseRepository = baseRepository ?? unitOfWork.GetRepository<TEntity>();
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

        TEntity addedEntity = await BaseRepository.AddAsync(entity);

        return addedEntity;
    }

    public virtual async Task<TEntity?> UpdateAsync(long id, TEntity entity)
    {
        await ValidateAsync(entity, false);

        TEntity? updatedEntity = await BaseRepository.UpdateAsync(id, entity);

        if (updatedEntity == null)
        {
            return null;
        }

        return updatedEntity;
    }

    public virtual async Task<bool> DeleteAsync(long id)
    {
        bool deleted = await BaseRepository.DeleteAsync(id);

        if (!deleted)
        {
            return false;
        }

        return true;
    }

    public virtual async Task<bool> ArchiveAsync(long id)
    {
        bool archived = await BaseRepository.ArchiveAsync(id);

        if (!archived)
        {
            return false;
        }

        return true;
    }

    public virtual async Task<bool> UnarchiveAsync(long id)
    {
        bool unarchived = await BaseRepository.UnarchiveAsync(id);

        if (!unarchived)
        {
            return false;
        }

        return true;
    }

    protected virtual Task ValidateAsync(TEntity entity, bool isCreate)
    {
        return Task.CompletedTask;
    }
}

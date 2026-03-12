using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;

namespace FamilyTree.Repositories.Core;

public interface IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    public Task<TEntity?> GetAsync(long id);

    public Task<List<TEntity>> GetAllAsync();

    public Task<FilterList<TEntity>> GetListAsync(
        BaseFilterRequest filterRequest,
        params string[] searchableProperties);

    public Task<TEntity> AddAsync(TEntity entity);

    public Task<TEntity?> UpdateAsync(long id, TEntity entity);

    public Task<bool> DeleteAsync(long id);

    public Task<bool> ArchiveAsync(long id);

    public Task<bool> UnarchiveAsync(long id);
}

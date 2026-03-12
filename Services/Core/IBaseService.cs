using FamilyTree.Entity;
using FamilyTree.Features.Pagination;

namespace FamilyTree.Services.Core;

public interface IBaseService<TEntity, in TFilter>
    where TEntity : BaseEntity
    where TFilter : class
{
    public Task<TEntity?> GetAsync(long id);

    public Task<List<TEntity>> GetAllAsync();

    public Task<FilterList<TEntity>> GetListAsync(TFilter filterRequest);

    public Task<TEntity?> AddAsync(TEntity entity);

    public Task<TEntity?> UpdateAsync(long id, TEntity entity);

    public Task<bool> DeleteAsync(long id);

    public Task<bool> ArchiveAsync(long id);

    public Task<bool> UnarchiveAsync(long id);
}

using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly FamilyTreeContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public BaseRepository(FamilyTreeContext dbContext)
    {
        DbContext = dbContext;
        DbSet = DbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetAsync(long id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await DbSet
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public virtual async Task<FilterList<TEntity>> GetListAsync(
        BaseFilterRequest filterRequest,
        params string[] searchableProperties)
    {
        IQueryable<TEntity> query = DbSet.AsQueryable();

        query = BaseFilterRequestQueryBuilder<TEntity>.ApplyFilters(
            query,
            filterRequest.Filter,
            filterRequest.Archived);

        query = BaseFilterRequestQueryBuilder<TEntity>.ApplyTermFilter(
            query,
            filterRequest.Term,
            searchableProperties);

        int totalCount = await query.CountAsync();

        List<TEntity> items = await query
            .OrderByDescending(x => x.Id)
            .Skip(filterRequest.Page * filterRequest.PerPage)
            .Take(filterRequest.PerPage)
            .ToListAsync();

        return new FilterList<TEntity>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filterRequest.Page,
            PerPage = filterRequest.PerPage
        };
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
        await DbContext.SaveChangesAsync();

        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(long id, TEntity entity)
    {
        TEntity? existingEntity = await GetAsync(id);

        if (existingEntity == null)
        {
            return null;
        }

        DbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
        existingEntity.Id = id;

        await DbContext.SaveChangesAsync();

        return existingEntity;
    }

    public virtual async Task<bool> DeleteAsync(long id)
    {
        TEntity? existingEntity = await GetAsync(id);

        if (existingEntity == null)
        {
            return false;
        }

        existingEntity.Deleted = true;
        await DbContext.SaveChangesAsync();

        return true;
    }

    public virtual async Task<bool> ArchiveAsync(long id)
    {
        TEntity? existingEntity = await GetAsync(id);

        if (existingEntity == null)
        {
            return false;
        }

        existingEntity.Archived = true;
        await DbContext.SaveChangesAsync();

        return true;
    }

    public virtual async Task<bool> UnarchiveAsync(long id)
    {
        TEntity? existingEntity = await GetAsync(id);

        if (existingEntity == null)
        {
            return false;
        }

        existingEntity.Archived = false;
        await DbContext.SaveChangesAsync();

        return true;
    }
}

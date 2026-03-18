using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class FamilyRepository : BaseRepository<Family>, IFamilyRepository
{
    public FamilyRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<FilterList<Family>> GetPublicFamiliesPagedAsync(FamilyFilterRequest filterRequest)
    {
        IQueryable<Family> query = DbContext.Families
            .AsNoTracking()
            .Where(family => family.IsPublic);

        if (!string.IsNullOrWhiteSpace(filterRequest.Term))
        {
            string term = filterRequest.Term.ToLower();

            query = query.Where(family =>
                family.Name.ToLower().Contains(term) ||
                family.Slug.ToLower().Contains(term) ||
                (family.OriginPlace != null && family.OriginPlace.ToLower().Contains(term)) ||
                (family.SeoTitle != null && family.SeoTitle.ToLower().Contains(term)));
        }

        int totalCount = await query.CountAsync();

        List<Family> items = await query
            .OrderBy(family => family.Name)
            .Skip(filterRequest.Page * filterRequest.PerPage)
            .Take(filterRequest.PerPage)
            .Include(family => family.Persons)
            .ToListAsync();

        return new FilterList<Family>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filterRequest.Page,
            PerPage = filterRequest.PerPage
        };
    }

    public async Task<Family?> GetPublicBySlugAsync(string slug)
    {
        string normalizedSlug = slug.Trim().ToLower();

        return await DbContext.Families
            .AsNoTracking()
            .Include(family => family.Persons)
            .FirstOrDefaultAsync(family =>
                family.IsPublic &&
                family.Slug.ToLower() == normalizedSlug);
    }
}

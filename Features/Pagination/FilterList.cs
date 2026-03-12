namespace FamilyTree.Features.Pagination;

public class FilterList<TEntity>
    where TEntity : class
{
    public IList<TEntity> Items { get; set; } = new List<TEntity>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PerPage { get; set; }
}

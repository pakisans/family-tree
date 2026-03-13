namespace FamilyTree.Entity;


public class Family : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? OriginPlace { get; set; }

    public bool IsPublic { get; set; } = false;

    public bool AllowPublicTree { get; set; } = false;

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public ICollection<Person> Persons { get; set; } = new List<Person>();
}

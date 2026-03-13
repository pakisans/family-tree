namespace FamilyTree.Entity;

public class Union : BaseEntity
{
    public long Person1Id { get; set; }

    public Person? Person1 { get; set; }

    public long Person2Id { get; set; }

    public Person? Person2 { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}

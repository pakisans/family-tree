using FamilyTree.Constants;

namespace FamilyTree.Entity;

public class Relationship : BaseEntity
{
    public long FromPersonId { get; set; }

    public Person? FromPerson { get; set; }

    public long ToPersonId { get; set; }

    public Person? ToPerson { get; set; }

    public RelationshipType RelationshipType { get; set; } = RelationshipType.Unknown;

    public string? Notes { get; set; }
}

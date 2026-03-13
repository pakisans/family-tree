namespace FamilyTree.Features.Filtering;

public class RelationshipFilterRequest : BaseFilterRequest
{
    public long? FromPersonId { get; set; }

    public long? ToPersonId { get; set; }
}

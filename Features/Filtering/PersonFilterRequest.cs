namespace FamilyTree.Features.Filtering;

public class PersonFilterRequest : BaseFilterRequest
{
    public long? FamilyId { get; set; }
}

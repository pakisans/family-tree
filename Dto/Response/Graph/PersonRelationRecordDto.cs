namespace FamilyTree.Dto.Response.Graph;

public class PersonRelationRecordDto
{
    public long SourcePersonId { get; set; }

    public long TargetPersonId { get; set; }

    public string EdgeType { get; set; } = string.Empty;
}

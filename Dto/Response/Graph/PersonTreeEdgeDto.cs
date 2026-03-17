namespace FamilyTree.Dto.Response.Graph;

public class PersonTreeEdgeDto
{
    public string Id { get; set; } = string.Empty;

    public long SourceId { get; set; }

    public long TargetId { get; set; }

    public string EdgeType { get; set; } = string.Empty;
}

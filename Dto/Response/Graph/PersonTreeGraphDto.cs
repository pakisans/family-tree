using FamilyTree.Dto.Response.Graph;
public class PersonTreeGraphDto
{
    public long RootPersonId { get; set; }

    public int UpDepth { get; set; }

    public int DownDepth { get; set; }

    public bool IncludePartners { get; set; }

    public bool IncludeSiblings { get; set; }

    public IList<PersonTreeNodeDto> Nodes { get; set; } = new List<PersonTreeNodeDto>();

    public IList<PersonTreeEdgeDto> Edges { get; set; } = new List<PersonTreeEdgeDto>();
}

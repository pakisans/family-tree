namespace FamilyTree.Dto;

public class PersonTreeDto
{
    public PersonSummaryDto? Person { get; set; }

    public IList<PersonSummaryDto> Parents { get; set; } = new List<PersonSummaryDto>();

    public IList<PersonSummaryDto> Children { get; set; } = new List<PersonSummaryDto>();

    public IList<PersonSummaryDto> FamilyMembers { get; set; } = new List<PersonSummaryDto>();
}

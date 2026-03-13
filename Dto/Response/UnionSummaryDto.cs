namespace FamilyTree.Dto;

public class UnionSummaryDto
{
    public long UnionId { get; set; }

    public PersonSummaryDto? Partner { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }
}

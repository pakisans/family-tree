using FamilyTree.Dtos;

namespace FamilyTree.Dto;

public class UnionDto : BaseDto
{
    public long Person1Id { get; set; }

    public long Person2Id { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }
}

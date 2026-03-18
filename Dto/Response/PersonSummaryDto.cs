using FamilyTree.Constants;

namespace FamilyTree.Dto;

public class PersonSummaryDto
{
    public long Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public DateTime? DeathDate { get; set; }

    public Gender Gender { get; set; }

    public long? FamilyId { get; set; }

    public string? BirthPlace { get; set; }

    public bool IsPublic { get; set; }
}

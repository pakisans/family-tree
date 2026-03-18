using FamilyTree.Constants;

namespace FamilyTree.Dto.Family;

public class FamilyMemberRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public DateTime? DeathDate { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    public string? BirthPlace { get; set; }

    public string? DeathPlace { get; set; }

    public string? Biography { get; set; }

    public bool IsPublic { get; set; } = false;
}

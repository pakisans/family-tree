using FamilyTree.Constants;
using FamilyTree.Dtos;

namespace FamilyTree.Dto;

public class PersonSummaryDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public DateTime? DeathDate { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    public long? FamilyId { get; set; }
}

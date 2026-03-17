using FamilyTree.Constants;

namespace FamilyTree.Dto.Response.Graph;

public class PersonTreeNodeDto
{
    public long Id { get; set; }

    public string NodeType { get; set; } = "person";

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public DateTime? DeathDate { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    public long? FamilyId { get; set; }

    public bool IsRoot { get; set; }

    public bool IsPublic { get; set; }

    public int Level { get; set; }
}

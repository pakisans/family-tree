using FamilyTree.Constants;

namespace FamilyTree.Dto.Family;

public class FamilyCollaboratorDto
{
    public long Id { get; set; }
    public long FamilyId { get; set; }
    public long UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public FamilyAccessRole AccessRole { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public long? InvitedByUserId { get; set; }
    public bool IsActive { get; set; }
}

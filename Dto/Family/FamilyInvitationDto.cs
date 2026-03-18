using FamilyTree.Constants;

namespace FamilyTree.Dto.Family;

public class FamilyInvitationDto
{
    public long Id { get; set; }
    public long FamilyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public FamilyAccessRole AccessRole { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public long InvitedByUserId { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime DateCreated { get; set; }
}

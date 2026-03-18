using FamilyTree.Constants;

namespace FamilyTree.Entity;

public class FamilyInvitation : BaseEntity
{
    public long FamilyId { get; set; }

    public Family? Family { get; set; }

    public string Email { get; set; } = string.Empty;

    public FamilyAccessRole AccessRole { get; set; } = FamilyAccessRole.ReadOnly;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public long InvitedByUserId { get; set; }

    public User? InvitedByUser { get; set; }
}

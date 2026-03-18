using FamilyTree.Constants;

namespace FamilyTree.Entity;

public class FamilyAccess : BaseEntity
{
    public long FamilyId { get; set; }

    public Family? Family { get; set; }

    public long UserId { get; set; }

    public User? User { get; set; }

    public FamilyAccessRole AccessRole { get; set; } = FamilyAccessRole.ReadOnly;

    public long InvitedByUserId { get; set; }

    public User? InvitedByUser { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public bool IsActive { get; set; } = true;
}

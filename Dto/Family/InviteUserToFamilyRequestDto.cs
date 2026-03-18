using FamilyTree.Constants;

namespace FamilyTree.Dto.Family;

public class InviteUserToFamilyRequestDto
{
    public string Email { get; set; } = string.Empty;

    public FamilyAccessRole AccessRole { get; set; } = FamilyAccessRole.ReadOnly;
}

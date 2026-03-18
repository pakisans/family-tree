using FamilyTree.Constants;
using FamilyTree.Entity;

namespace FamilyTree.Services.Core;

public interface IFamilyAuthorizationService
{
    public Task EnsureCanReadFamilyAsync(Family family);

    public Task EnsureCanEditFamilyAsync(Family family);

    public Task EnsureCanManageFamilyAccessAsync(Family family);

    public Task<FamilyAccessRole?> GetFamilyAccessRoleAsync(long familyId, long userId);

    public Task EnsureCanReadFamilyByIdAsync(long familyId);

    public Task EnsureCanEditFamilyByIdAsync(long familyId);

    public Task EnsureCanManageFamilyAccessByIdAsync(long familyId);
}

namespace FamilyTree.Services.Core.Auth;

public interface ICurrentUserService
{
    public long? GetCurrentUserId();

    public bool IsAuthenticated();

    public bool IsInRole(string roleName);
}

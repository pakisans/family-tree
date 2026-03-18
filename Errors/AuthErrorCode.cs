namespace FamilyTree.Errors;

public static class AuthErrorCode
{
    public const string AUTH_0001 = "AUTH_0001";
    public const string AUTH_0002 = "AUTH_0002";
    public const string AUTH_0003 = "AUTH_0003";
    public const string AUTH_0004 = "AUTH_0004";
    public const string AUTH_0005 = "AUTH_0005";
    public const string AUTH_0006 = "AUTH_0006";
    public const string AUTH_0007 = "AUTH_0007";
    public const string AUTH_0008 = "AUTH_0008";

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { AUTH_0001, "Invalid email or password." },
        { AUTH_0002, "User with this email already exists." },
        { AUTH_0003, "Refresh token is invalid." },
        { AUTH_0004, "Refresh token is no longer valid." },
        { AUTH_0005, "Invitation expired." },
        { AUTH_0006, "User with invitation email does not exist." },
        { AUTH_0007, "Default user role is missing." },
        { AUTH_0008, "User account is inactive." }
    };

    public static string GetDescription(string errorCode)
    {
        return Descriptions.TryGetValue(errorCode, out string? description)
            ? description
            : "Unknown auth error.";
    }
}

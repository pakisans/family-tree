namespace FamilyTree.Errors;

public static class BaseErrorCode
{
    public const string BASE_0001 = "BASE_0001";
    public const string BASE_0002 = "BASE_0002";
    public const string BASE_0003 = "BASE_0003";
    public const string BASE_0004 = "BASE_0004";
    public const string BASE_0005 = "BASE_0005";
    public const string BASE_0006 = "BASE_0006";
    public const string BASE_0007 = "BASE_0007";
    public const string BASE_0008 = "BASE_0008";

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { BASE_0001, "Object not found." },
        { BASE_0002, "Object already exists." },
        { BASE_0003, "Invalid request." },
        { BASE_0004, "Unauthorized." },
        { BASE_0005, "Forbidden." },
        { BASE_0006, "Authentication required." },
        { BASE_0007, "Missing required data." },
        { BASE_0008, "Internal server error." }
    };

    public static string GetDescription(string errorCode)
    {
        return Descriptions.TryGetValue(errorCode, out string? description)
            ? description
            : "Unknown error code.";
    }
}

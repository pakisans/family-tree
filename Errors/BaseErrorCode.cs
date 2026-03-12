namespace FamilyTree.Errors;

public static class BaseErrorCode
{
    public const string BASE_0001 = "BASE_0001";
    public const string BASE_0002 = "BASE_0002";
    public const string BASE_0003 = "BASE_0003";
    public const string BASE_0004 = "BASE_0004";

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { BASE_0001, "Object not found." },
        { BASE_0002, "Object already exists." },
        { BASE_0003, "Invalid request." },
        { BASE_0004, "Unauthorized." }
    };

    public static string GetDescription(string errorCode)
    {
        return Descriptions.TryGetValue(errorCode, out string? description)
            ? description
            : "Unknown error code.";
    }
}

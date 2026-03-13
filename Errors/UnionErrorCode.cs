namespace FamilyTree.Errors;

public static class UnionErrorCode
{
    public const string UNION_0001 = "UNION_0001";
    public const string UNION_0002 = "UNION_0002";
    public const string UNION_0003 = "UNION_0003";
    public const string UNION_0004 = "UNION_0004";
    public const string UNION_0005 = "UNION_0005";
    public const string UNION_0006 = "UNION_0006";

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { UNION_0001, "A person cannot create a union with itself." },
        { UNION_0002, "First person does not exist." },
        { UNION_0003, "Second person does not exist." },
        { UNION_0004, "Union already exists." },
        { UNION_0005, "Union end date cannot be before start date." },
        { UNION_0006, "Only one active union between the same persons is allowed." }
    };

    public static string GetDescription(string errorCode)
    {
        return Descriptions.TryGetValue(errorCode, out string? description)
            ? description
            : "Unknown union error.";
    }
}

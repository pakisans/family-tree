namespace FamilyTree.Errors;

public static class RelationshipErrorCode
{
    public const string RELATIONSHIP_0001 = "RELATIONSHIP_0001";
    public const string RELATIONSHIP_0002 = "RELATIONSHIP_0002";
    public const string RELATIONSHIP_0003 = "RELATIONSHIP_0003";
    public const string RELATIONSHIP_0004 = "RELATIONSHIP_0004";
    public const string RELATIONSHIP_0005 = "RELATIONSHIP_0005";

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { RELATIONSHIP_0001, "Relationship type is required." },
        { RELATIONSHIP_0002, "A person cannot have a relationship with itself." },
        { RELATIONSHIP_0003, "Source person does not exist." },
        { RELATIONSHIP_0004, "Target person does not exist." },
        { RELATIONSHIP_0005, "Relationship already exists." }
    };

    public static string GetDescription(string errorCode)
    {
        return Descriptions.TryGetValue(errorCode, out string? description)
            ? description
            : "Unknown relationship error.";
    }
}

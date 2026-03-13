using FamilyTree.Constants;
using FamilyTree.Dtos;

namespace FamilyTree.Dto;

public class RelationshipDto : BaseDto
{
    public long FromPersonId { get; set; }

    public long ToPersonId { get; set; }

    public RelationshipType RelationshipType { get; set; } = RelationshipType.Unknown;

    public string? Notes { get; set; }
}

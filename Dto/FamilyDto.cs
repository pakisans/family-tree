using FamilyTree.Dtos;

namespace FamilyTree.Dto;

public class FamilyDto : BaseDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? OriginPlace { get; set; }

    public bool IsPublic { get; set; }

    public bool AllowPublicTree { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }
}

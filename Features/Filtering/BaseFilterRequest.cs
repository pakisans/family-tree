using System.ComponentModel.DataAnnotations;

namespace FamilyTree.Features.Filtering;

public class BaseFilterRequest
{
    public BaseFilterRequest()
    {
    }

    public BaseFilterRequest(int page)
    {
        Page = page;
    }

    [Required(ErrorMessage = "Page is mandatory")]
    public int Page { get; init; } = 0;

    [Required(ErrorMessage = "PerPage is mandatory")]
    public int PerPage { get; set; } = 20;

    public string? Term { get; set; }

    public bool Archived { get; set; } = false;

    public Dictionary<string, string>? Filter { get; set; } = new();

    public long? CurrentUser { get; set; }

    public long? Id { get; set; }
}

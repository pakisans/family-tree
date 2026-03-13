using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[Route("api/unions")]
public class UnionController : BaseController<Union, UnionDto, UnionFilterRequest>
{
    private readonly IUnionService _unionService;

    public UnionController(
        IUnionService baseService,
        IUnionConverter baseConverter)
        : base(baseService, baseConverter)
    {
        _unionService = baseService;
    }

    [HttpGet("person/{personId:long}/partners")]
    public async Task<IActionResult> GetPartners([FromRoute] long personId)
    {
        IList<UnionSummaryDto> partners = await _unionService.GetPartnersAsync(personId);
        return Ok(partners);
    }
}

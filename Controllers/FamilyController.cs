using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[Route("api/families")]
public class FamilyController : BaseController<Family, FamilyDto, FamilyFilterRequest>
{
    private readonly IFamilyService _familyService;

    public FamilyController(
        IFamilyService baseService,
        IFamilyConverter baseConverter)
        : base(baseService, baseConverter)
    {
        _familyService = baseService;
    }

    [HttpGet("{id:long}/members")]
    public async Task<IActionResult> GetMembers([FromRoute] long id)
    {
        IList<PersonSummaryDto> members = await _familyService.GetMembersAsync(id);
        return Ok(members);
    }
}

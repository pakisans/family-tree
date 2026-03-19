using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Dto.Family;
using FamilyTree.Dto.Response.Graph;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[ApiController]
[Authorize]
[Route("api/families")]
public class FamilyController : BaseController<Family, FamilyDto, FamilyFilterRequest>
{
    private readonly IFamilyService _familyService;
    private readonly IPersonService _personService;

    public FamilyController(
        IFamilyService baseService,
        IFamilyConverter baseConverter,
        IPersonService personService)
        : base(baseService, baseConverter)
    {
        _familyService = baseService;
        _personService = personService;
    }

    [HttpGet("{id:long}/members")]
    public async Task<IActionResult> GetMembers(
        [FromRoute] long id,
        [FromQuery] PersonFilterRequest filterRequest)
    {
        FilterList<PersonSummaryDto> members = await _familyService.GetMembersAsync(id, filterRequest);
        return Ok(members);
    }

    [HttpGet("{familyId:long}/members/{personId:long}")]
    public async Task<IActionResult> GetMember(
        [FromRoute] long familyId,
        [FromRoute] long personId)
    {
        PersonDto? member = await _familyService.GetMemberAsync(familyId, personId);

        if (member == null)
        {
            return NotFound();
        }

        return Ok(member);
    }

    [HttpPost("{id:long}/members")]
    public async Task<IActionResult> AddMember(
        [FromRoute] long id,
        [FromBody] FamilyMemberRequestDto request)
    {
        PersonDto? createdPerson = await _familyService.AddMemberAsync(id, request);

        if (createdPerson == null)
        {
            return BadRequest();
        }

        return Ok(createdPerson);
    }

    [HttpPut("{familyId:long}/members/{personId:long}")]
    public async Task<IActionResult> UpdateMember(
        [FromRoute] long familyId,
        [FromRoute] long personId,
        [FromBody] FamilyMemberRequestDto request)
    {
        PersonDto? updatedPerson = await _familyService.UpdateMemberAsync(familyId, personId, request);

        if (updatedPerson == null)
        {
            return NotFound();
        }

        return Ok(updatedPerson);
    }

    [HttpGet("{id:long}/graph")]
    public async Task<IActionResult> GetFamilyGraph([FromRoute] long id)
    {
        PersonTreeGraphDto graph = await _personService.GetFamilyGraphAsync(id);
        return Ok(graph);
    }
}

using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[Route("api/persons")]
public class PersonController : BaseController<Person, PersonDto, PersonFilterRequest>
{
    private readonly IPersonService _personService;

    public PersonController(
        IPersonService baseService,
        IPersonConverter baseConverter)
        : base(baseService, baseConverter)
    {
        _personService = baseService;
    }

    [HttpGet("{id:long}/parents")]
    public async Task<IActionResult> GetParents([FromRoute] long id)
    {
        IList<PersonSummaryDto> parents = await _personService.GetParentsAsync(id);
        return Ok(parents);
    }

    [HttpGet("{id:long}/children")]
    public async Task<IActionResult> GetChildren([FromRoute] long id)
    {
        IList<PersonSummaryDto> children = await _personService.GetChildrenAsync(id);
        return Ok(children);
    }
}

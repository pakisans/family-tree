using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Features.Pagination;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPersonService _personService;
    private readonly IFamilyConverter _familyConverter;

    public PublicController(
        IFamilyRepository familyRepository,
        IPersonRepository personRepository,
        IPersonService personService,
        IFamilyConverter familyConverter)
    {
        _familyRepository = familyRepository;
        _personRepository = personRepository;
        _personService = personService;
        _familyConverter = familyConverter;
    }

    [HttpGet("families")]
    public async Task<IActionResult> GetPublicFamilies([FromQuery] FamilyFilterRequest filterRequest)
    {
        FilterList<Family> publicFamilies = await _familyRepository.GetPublicFamiliesPagedAsync(filterRequest);

        FilterList<FamilyDto> result = new FilterList<FamilyDto>
        {
            Items = _familyConverter.MapToDtos(publicFamilies.Items).ToList(),
            TotalCount = publicFamilies.TotalCount,
            Page = publicFamilies.Page,
            PerPage = publicFamilies.PerPage
        };

        return Ok(result);
    }

    [HttpGet("families/{slug}")]
    public async Task<IActionResult> GetFamilyBySlug([FromRoute] string slug)
    {
        Family? family = await _familyRepository.GetPublicBySlugAsync(slug);

        if (family == null)
        {
            return NotFound(new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."));
        }

        return Ok(_familyConverter.MapToDto(family));
    }

    [HttpGet("families/{slug}/members")]
    public async Task<IActionResult> GetFamilyMembersBySlug(
        [FromRoute] string slug,
        [FromQuery] PersonFilterRequest filterRequest)
    {
        Family? family = await _familyRepository.GetPublicBySlugAsync(slug);

        if (family == null)
        {
            return NotFound(new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."));
        }

        FilterList<PersonSummaryDto> members =
            await _personRepository.GetPublicFamilyMembersPagedAsync(family.Id, filterRequest);

        return Ok(members);
    }

    [HttpGet("families/{slug}/graph/{personId:long}")]
    public async Task<IActionResult> GetPublicPersonGraph(
        [FromRoute] string slug,
        [FromRoute] long personId,
        [FromQuery] int up = 3,
        [FromQuery] int down = 3,
        [FromQuery] bool includePartners = true,
        [FromQuery] bool includeSiblings = false)
    {
        Family? family = await _familyRepository.GetPublicBySlugAsync(slug);

        if (family == null)
        {
            return NotFound(new ErrorResponse(BaseErrorCode.BASE_0001, "Family not found."));
        }

        Person? person = await _personRepository.GetAsync(personId);

        if (person == null || person.FamilyId != family.Id || !person.IsPublic)
        {
            return NotFound(new ErrorResponse(BaseErrorCode.BASE_0001, "Person not found."));
        }

        PersonTreeGraphDto graph =
            await _personService.GetGraphAsync(personId, up, down, includePartners, includeSiblings);

        HashSet<long> publicPersonIds = graph.Nodes
            .Where(node => node.IsPublic)
            .Select(node => node.Id)
            .ToHashSet();

        graph.Nodes = graph.Nodes
            .Where(node => publicPersonIds.Contains(node.Id))
            .ToList();

        graph.Edges = graph.Edges
            .Where(edge => publicPersonIds.Contains(edge.SourceId) && publicPersonIds.Contains(edge.TargetId))
            .ToList();

        return Ok(graph);
    }
}

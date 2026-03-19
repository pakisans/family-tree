using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;
using FamilyTree.Features.Filtering;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[ApiController]
[Authorize]
[Route("api/relationships")]
public class RelationshipController : BaseController<Relationship, RelationshipDto, RelationshipFilterRequest>
{
    public RelationshipController(
        IRelationshipService baseService,
        IRelationshipConverter baseConverter)
        : base(baseService, baseConverter)
    {
    }
}

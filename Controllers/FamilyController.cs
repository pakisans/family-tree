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
    public FamilyController(
        IFamilyService baseService,
        IFamilyConverter baseConverter)
        : base(baseService, baseConverter)
    {
    }
}

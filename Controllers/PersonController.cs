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
    public PersonController(
        IPersonService baseService,
        IPersonConverter baseConverter) : base(baseService, baseConverter)
    {
    }
}

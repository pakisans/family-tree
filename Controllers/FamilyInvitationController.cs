using FamilyTree.Dto.Family;
using FamilyTree.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[ApiController]
[Authorize]
[Route("api/families/{familyId:long}")]
public class FamilyInvitationController : ControllerBase
{
    private readonly IFamilyInvitationService _familyInvitationService;

    public FamilyInvitationController(IFamilyInvitationService familyInvitationService)
    {
        _familyInvitationService = familyInvitationService;
    }

    [HttpGet("collaborators")]
    public async Task<IActionResult> GetCollaborators([FromRoute] long familyId)
    {
        IList<FamilyCollaboratorDto> collaborators = await _familyInvitationService.GetCollaboratorsAsync(familyId);
        return Ok(collaborators);
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> GetInvitations([FromRoute] long familyId)
    {
        IList<FamilyInvitationDto> invitations = await _familyInvitationService.GetInvitationsAsync(familyId);
        return Ok(invitations);
    }

    [HttpPost("invitations")]
    public async Task<IActionResult> Invite([FromRoute] long familyId, [FromBody] InviteUserToFamilyRequestDto request)
    {
        await _familyInvitationService.InviteAsync(familyId, request);
        return Ok();
    }

    [HttpDelete("invitations/{invitationId:long}")]
    public async Task<IActionResult> RevokeInvitation([FromRoute] long familyId, [FromRoute] long invitationId)
    {
        await _familyInvitationService.RevokeInvitationAsync(familyId, invitationId);
        return Ok();
    }

    [HttpDelete("collaborators/{collaboratorId:long}")]
    public async Task<IActionResult> RemoveCollaborator([FromRoute] long familyId, [FromRoute] long collaboratorId)
    {
        await _familyInvitationService.RemoveCollaboratorAsync(familyId, collaboratorId);
        return Ok();
    }
}

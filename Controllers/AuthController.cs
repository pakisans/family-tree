using FamilyTree.Dto.Auth;
using FamilyTree.Services.Core.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        AuthResponseDto response = await _authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        AuthResponseDto response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        AuthResponseDto response = await _authService.RefreshAsync(request);
        return Ok(response);
    }

    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequestDto request)
    {
        await _authService.AcceptInvitationAsync(request);
        return Ok();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        CurrentUserDto? currentUser = await _authService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            return Unauthorized();
        }

        return Ok(currentUser);
    }
}

using FamilyTree.Dto.Auth;

namespace FamilyTree.Services.Core.Auth;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    public Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    public Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request);

    public Task AcceptInvitationAsync(AcceptInvitationRequestDto request);

    public Task<CurrentUserDto?> GetCurrentUserAsync();
}

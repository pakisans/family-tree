using FamilyTree.Dto.Auth;
using FamilyTree.Entity;

namespace FamilyTree.Services.Core.Auth;

public interface ITokenService
{
    public Task<AuthResponseDto> CreateAuthResponseAsync(User user);

    public string GenerateRefreshToken();
}

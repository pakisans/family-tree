using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using family_tree.Configuration;
using FamilyTree.Dto.Auth;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core.Auth;
using Microsoft.IdentityModel.Tokens;

namespace FamilyTree.Services.Auth;

public class TokenService : ITokenService
{
    private readonly ISystemConfiguration _systemConfiguration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(
        ISystemConfiguration systemConfiguration,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _systemConfiguration = systemConfiguration;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> CreateAuthResponseAsync(User user)
    {
        IList<string> roles = user.UserRoles
            .Where(userRole => userRole.Role != null)
            .Select(userRole => userRole.Role!.Name)
            .Distinct()
            .ToList();

        DateTime accessTokenExpiresAt = DateTime.UtcNow.AddHours(2);

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("security_stamp", user.SecurityStamp)
        };

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_systemConfiguration.Jwt.Key));

        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtToken = new JwtSecurityToken(
            issuer: _systemConfiguration.Jwt.Authority,
            audience: _systemConfiguration.Jwt.Authority,
            claims: claims,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        string refreshTokenValue = GenerateRefreshToken();

        RefreshToken refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsUsed = false
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = accessTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            Roles = roles
        };
    }

    public string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}

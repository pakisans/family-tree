using System.Net;
using FamilyTree.Constants;
using FamilyTree.Dto.Auth;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core.Auth;
using Microsoft.AspNetCore.Identity;

namespace FamilyTree.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0007, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0007)),
                HttpStatusCode.BadRequest);
        }

        string normalizedEmail = request.Email.Trim().ToLower();

        User? existingUser = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0002, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0002)),
                HttpStatusCode.Conflict);
        }

        Role? userRole = await _unitOfWork.Roles.GetByNameAsync(GlobalRoleName.User);

        if (userRole == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0007, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0007)),
                HttpStatusCode.InternalServerError);
        }

        User user = new User
        {
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _unitOfWork.Users.AddAsync(user);

        UserRole userRoleLink = new UserRole
        {
            UserId = user.Id,
            RoleId = userRole.Id
        };

        await _unitOfWork.GetRepository<UserRole>().AddAsync(userRoleLink);

        User? createdUser = await _unitOfWork.Users.GetWithRolesAsync(user.Id);

        if (createdUser == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0008, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0008)),
                HttpStatusCode.InternalServerError);
        }

        return await _tokenService.CreateAuthResponseAsync(createdUser);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0007, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0007)),
                HttpStatusCode.BadRequest);
        }

        string normalizedEmail = request.Email.Trim().ToLower();

        User? user = await _unitOfWork.Users.GetByEmailWithRolesAsync(normalizedEmail);

        if (user == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0001, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0001)),
                HttpStatusCode.Unauthorized);
        }

        if (!user.IsActive)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0008, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0008)),
                HttpStatusCode.Forbidden);
        }

        PasswordVerificationResult verificationResult =
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0001, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0001)),
                HttpStatusCode.Unauthorized);
        }

        return await _tokenService.CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0007, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0007)),
                HttpStatusCode.BadRequest);
        }

        RefreshToken? refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

        if (refreshToken == null || refreshToken.User == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0003, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0003)),
                HttpStatusCode.Unauthorized);
        }

        if (refreshToken.IsExpired || refreshToken.IsRevoked || refreshToken.IsUsed)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0004, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0004)),
                HttpStatusCode.Unauthorized);
        }

        refreshToken.IsUsed = true;
        await _unitOfWork.CompleteAsync();

        User? user = await _unitOfWork.Users.GetWithRolesAsync(refreshToken.UserId);

        if (user == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        if (!user.IsActive)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0008, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0008)),
                HttpStatusCode.Forbidden);
        }

        return await _tokenService.CreateAuthResponseAsync(user);
    }

    public async Task AcceptInvitationAsync(AcceptInvitationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0007, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0007)),
                HttpStatusCode.BadRequest);
        }

        FamilyInvitation? invitation = await _unitOfWork.FamilyInvitations.GetPendingByTokenAsync(request.Token);

        if (invitation == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, "Invitation not found."),
                HttpStatusCode.NotFound);
        }

        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _unitOfWork.CompleteAsync();

            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0005, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0005)),
                HttpStatusCode.BadRequest);
        }

        User? user = await _unitOfWork.Users.GetByEmailAsync(invitation.Email);

        if (user == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(AuthErrorCode.AUTH_0006, AuthErrorCode.GetDescription(AuthErrorCode.AUTH_0006)),
                HttpStatusCode.NotFound);
        }

        FamilyAccess? existingAccess =
            await _unitOfWork.FamilyAccesses.GetActiveAccessAsync(invitation.FamilyId, user.Id);

        if (existingAccess == null)
        {
            FamilyAccess familyAccess = new FamilyAccess
            {
                FamilyId = invitation.FamilyId,
                UserId = user.Id,
                AccessRole = invitation.AccessRole,
                InvitedByUserId = invitation.InvitedByUserId,
                AcceptedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.FamilyAccesses.AddAsync(familyAccess);
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync();
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        long? currentUserId = _currentUserService.GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            return null;
        }

        User? user = await _unitOfWork.Users.GetWithRolesAsync(currentUserId.Value);

        if (user == null)
        {
            return null;
        }

        IList<string> roles = user.UserRoles
            .Where(userRole => userRole.Role != null)
            .Select(userRole => userRole.Role!.Name)
            .Distinct()
            .ToList();

        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed
        };
    }
}

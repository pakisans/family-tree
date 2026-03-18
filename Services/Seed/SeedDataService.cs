using family_tree.Configuration;
using FamilyTree.Constants;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core.Seed;
using Microsoft.AspNetCore.Identity;

namespace FamilyTree.Services.Seed;

public class SeedDataService : ISeedDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemConfiguration _systemConfiguration;
    private readonly PasswordHasher<User> _passwordHasher;

    public SeedDataService(
        IUnitOfWork unitOfWork,
        ISystemConfiguration systemConfiguration)
    {
        _unitOfWork = unitOfWork;
        _systemConfiguration = systemConfiguration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task SeedAsync()
    {
        Role superAdminRole = await EnsureRoleAsync(GlobalRoleName.SuperAdmin);
        Role userRole = await EnsureRoleAsync(GlobalRoleName.User);

        await EnsureSuperAdminAsync(superAdminRole, userRole);
    }

    private async Task<Role> EnsureRoleAsync(string roleName)
    {
        Role? existingRole = await _unitOfWork.Roles.GetByNameAsync(roleName);

        if (existingRole != null)
        {
            return existingRole;
        }

        Role role = new Role
        {
            Name = roleName
        };

        await _unitOfWork.Roles.AddAsync(role);

        return role;
    }

    private async Task EnsureSuperAdminAsync(Role superAdminRole, Role userRole)
    {
        if (string.IsNullOrWhiteSpace(_systemConfiguration.Admin.Email) ||
            string.IsNullOrWhiteSpace(_systemConfiguration.Admin.Password))
        {
            return;
        }

        string normalizedEmail = _systemConfiguration.Admin.Email.Trim().ToLower();

        User? existingUser = await _unitOfWork.Users.GetByEmailWithRolesAsync(normalizedEmail);

        if (existingUser == null)
        {
            User superAdminUser = new User
            {
                Email = normalizedEmail,
                FirstName = _systemConfiguration.Admin.FirstName.Trim(),
                LastName = _systemConfiguration.Admin.LastName.Trim(),
                EmailConfirmed = true,
                IsActive = true
            };

            superAdminUser.PasswordHash =
                _passwordHasher.HashPassword(superAdminUser, _systemConfiguration.Admin.Password);

            await _unitOfWork.Users.AddAsync(superAdminUser);

            await EnsureUserRoleAsync(superAdminUser.Id, superAdminRole.Id);
            await EnsureUserRoleAsync(superAdminUser.Id, userRole.Id);

            return;
        }

        bool hasSuperAdminRole = existingUser.UserRoles.Any(x => x.Role != null && x.Role.Name == GlobalRoleName.SuperAdmin);
        bool hasUserRole = existingUser.UserRoles.Any(x => x.Role != null && x.Role.Name == GlobalRoleName.User);

        if (!hasSuperAdminRole)
        {
            await EnsureUserRoleAsync(existingUser.Id, superAdminRole.Id);
        }

        if (!hasUserRole)
        {
            await EnsureUserRoleAsync(existingUser.Id, userRole.Id);
        }
    }

    private async Task EnsureUserRoleAsync(long userId, long roleId)
    {
        UserRole? existingUserRole = await _unitOfWork.GetRepository<UserRole>()
            .GetAsync(userRole => userRole.UserId == userId && userRole.RoleId == roleId);

        if (existingUserRole != null)
        {
            return;
        }

        UserRole userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        await _unitOfWork.GetRepository<UserRole>().AddAsync(userRole);
    }
}

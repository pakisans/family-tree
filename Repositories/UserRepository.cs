using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLower();

        return await DbContext.Users
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLower();

        return await DbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> GetWithRolesAsync(long userId)
    {
        return await DbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }
}

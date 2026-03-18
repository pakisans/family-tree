using FamilyTree.Database;
using FamilyTree.Entity;
using FamilyTree.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(FamilyTreeContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await DbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .ThenInclude(user => user!.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token);
    }
}

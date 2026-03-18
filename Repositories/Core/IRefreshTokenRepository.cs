using FamilyTree.Entity;

namespace FamilyTree.Repositories.Core;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    public Task<RefreshToken?> GetByTokenAsync(string token);
}

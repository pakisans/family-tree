using FamilyTree.Repositories.Core;

namespace FamilyTree.Repositories.Core;

public interface IUnitOfWork : IBaseUnitOfWork
{
    public IUserRepository Users { get; }

    public IRoleRepository Roles { get; }

    public IRefreshTokenRepository RefreshTokens { get; }

    public IFamilyAccessRepository FamilyAccesses { get; }

    public IFamilyInvitationRepository FamilyInvitations { get; }
}

using FamilyTree.Converters;
using FamilyTree.Converters.Core;
using FamilyTree.Repositories;
using FamilyTree.Repositories.Core;
using FamilyTree.Services;
using FamilyTree.Services.Auth;
using FamilyTree.Services.Core;
using FamilyTree.Services.Core.Auth;
using FamilyTree.Services.Core.Seed;
using FamilyTree.Services.Email;
using FamilyTree.Services.Seed;

namespace FamilyTree.Extensions;

public static class ServiceRegistrationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IPersonConverter, PersonConverter>();

        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyService, FamilyService>();
        services.AddScoped<IFamilyConverter, FamilyConverter>();

        services.AddScoped<IRelationshipRepository, RelationshipRepository>();
        services.AddScoped<IRelationshipService, RelationshipService>();
        services.AddScoped<IRelationshipConverter, RelationshipConverter>();

        services.AddScoped<IUnionRepository, UnionRepository>();
        services.AddScoped<IUnionService, UnionService>();
        services.AddScoped<IUnionConverter, UnionConverter>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IFamilyAccessRepository, FamilyAccessRepository>();
        services.AddScoped<IFamilyInvitationRepository, FamilyInvitationRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFamilyAuthorizationService, FamilyAuthorizationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFamilyInvitationService, FamilyInvitationService>();

        services.AddScoped<ISeedDataService, SeedDataService>();

        // Email system
        // EmailQueue and EmailSender are Singleton: they hold no per-request state
        // and must be shared across the lifetime of the application.
        // EmailService is Scoped (depends on ISystemConfiguration which is Singleton — fine).
        services.AddSingleton<IEmailQueue, EmailQueue>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<EmailBackgroundWorker>();
    }
}

using FamilyTree.Converters;
using FamilyTree.Converters.Core;
using FamilyTree.Repositories;
using FamilyTree.Repositories.Core;
using FamilyTree.Services;
using FamilyTree.Services.Core;

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
    }
}

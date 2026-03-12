using FamilyTree.Repositories;
using FamilyTree.Repositories.Core;

namespace FamilyTree.Extensions;

public static class ServiceRegistrationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
    }
}

using family_tree.Configuration;
using FamilyTree.Database;
using Microsoft.EntityFrameworkCore;

namespace FamilyTree.Extensions;

public static class DatabaseExtensions
{
    public static void AddDatabase(this WebApplicationBuilder builder, ISystemConfiguration systemConfiguration)
    {
        builder.Services.AddDbContext<FamilyTreeContext>(options =>
        {
            options.UseNpgsql(systemConfiguration.DatabaseConnection);
        });
    }

    public static void ApplyDatabaseMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        FamilyTreeContext dbContext = scope.ServiceProvider.GetRequiredService<FamilyTreeContext>();

        dbContext.Database.Migrate();
    }
}

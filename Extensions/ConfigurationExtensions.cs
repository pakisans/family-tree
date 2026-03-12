using family_tree.Configuration;

namespace FamilyTree.Extensions;

public static class ConfigurationExtensions
{
    public static ISystemConfiguration AddSystemConfiguration(this WebApplicationBuilder builder)
    {
        SystemConfiguration systemConfiguration = new SystemConfiguration();
        builder.Configuration.Bind(nameof(SystemConfiguration), systemConfiguration);

        builder.Services.AddSingleton<ISystemConfiguration>(systemConfiguration);

        return systemConfiguration;
    }
}

using System.Reflection;
using Frontline.Options;

namespace Frontline.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredOptions<TOptions>(this IServiceCollection services,
        IConfiguration config) where TOptions : class
    {
        var sectionAttribute = typeof(TOptions).GetCustomAttribute<OptionsSectionAttribute>();
        if (sectionAttribute is null)
        {
            throw new InvalidOperationException($"Missing OptionsSectionAttribute on {typeof(TOptions).Name}");
        }
        
        services
            .AddOptions<TOptions>()
            .Bind(config.GetSection(sectionAttribute.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
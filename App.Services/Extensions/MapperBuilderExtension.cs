using App.Services.Helpers;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace App.Services.Extensions;

/// <summary>
/// Provides extension methods for configuring AutoMapper services.
/// </summary>
public static class MapperBuilderExtension
{
    /// <summary>
    /// Registers AutoMapper with the application's service collection and adds the mapping profile.
    /// </summary>
    /// <param name="services">The service collection to add AutoMapper to.</param>
    /// <returns>The updated IServiceCollection with AutoMapper configured.</returns>
    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        // Create and configure the AutoMapper instance with a mapping profile
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MapperProfile()); // Add custom MapperProfile here
        });

        // Create a mapper instance using the configuration
        IMapper mapper = mappingConfig.CreateMapper();

        // Register the IMapper instance as a singleton in the DI container
        return services.AddSingleton(mapper);
    }
}

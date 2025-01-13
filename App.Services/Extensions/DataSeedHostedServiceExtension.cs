using App.Core.DataSeed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Services.Extensions;

/// <summary>
/// Provides extension methods for adding data seed services to the application.
/// </summary>
public static class DataSeedHostedServiceExtension
{
    /// <summary>
    /// Registers a hosted service to seed OpenId data during development.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext used for data access.</typeparam>
    /// <param name="services">The service collection to add the hosted service to.</param>
    /// <returns>The updated IServiceCollection with the development data seed service.</returns>
    public static IServiceCollection AddOpenIdDevelopmentDataSeed<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        // Add the hosted service that seeds OpenId data for development
        return services.AddHostedService<DevelopmentDataSeedHostedService<TContext>>();
    }
}

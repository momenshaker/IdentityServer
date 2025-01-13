using App.Core.Context;
using App.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Services.Extensions;

/// <summary>
/// Extension method to register services related to authentication, authorization, and database context.
/// Configures OpenIddict with JWT and Identity services, sets up DbContext, and registers necessary services for dependency injection.
/// </summary>
public static class ServicesRegisterExtension
{
    /// <summary>
    /// Registers services required for authentication, authorization, and database operations.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext to be used (typically ApplicationDbContext).</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration to access settings such as connection strings.</param>
    /// <returns>The updated IServiceCollection with the configured services.</returns>
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the application DbContext with a connection string and retry logic for SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());

            // Enable OpenIddict support on the DbContext
            options.UseOpenIddict();
        });

        // Register OpenIddict for authentication and authorization services
        services.AddOpenId<ApplicationDbContext>(configuration);

        // Register AutoMapper for object-to-object mapping
        services.AddAutoMapper();

        // Register scoped services for dependency injection
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ApplicationDbContext>();

        // Register the development data seed service to initialize data (development environment only)
        services.AddOpenIdDevelopmentDataSeed<ApplicationDbContext>();

        return services;
    }
}

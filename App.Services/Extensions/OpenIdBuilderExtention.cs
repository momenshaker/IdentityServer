using App.Core.Context;
using App.Core.Entites;
using App.Services.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace App.Services.Extensions;

/// <summary>
/// Provides extension methods for configuring OpenIddict with JWT and Identity services for authentication and authorization.
/// </summary>
public static class OpenIdExtension
{
    /// <summary>
    /// Configures OpenIddict with JWT and Identity services for authentication and authorization.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The application configuration for settings.</param>
    /// <returns>The updated IServiceCollection with OpenIddict and authentication services.</returns>
    public static OpenIddictBuilder AddOpenId<TContext>(this IServiceCollection services, IConfiguration configuration)
        where TContext : DbContext
    {
        // Register configuration options for Identity, Authorization, JWT Bearer, and OpenIddict
        services.AddSingleton<IConfigureOptions<IdentityOptions>, IdentityOptionsSetup>();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationOptionsSetup>();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
        services.AddSingleton<IConfigureOptions<OpenIddictServerBuilder>, OpenIddictServerBuilderSetup>();
        services.AddSingleton<IConfigureOptions<OpenIddictValidationBuilder>, OpenIddictValidationBuilderSetup>();

        // Configure Identity services for user authentication
        services.AddIdentity<AspNetUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserManager<UserManager<AspNetUser>>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        // Configure default Identity options (can be extended for customizations)
        services.Configure<IdentityOptions>(option => { });

        // Configure Authentication with JWT Bearer scheme
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Configure Authorization policies
        services.AddAuthorization();

        // Configure OpenIddict services for both core functionality and validation
        return services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseEntityFrameworkCore().UseDbContext<TContext>(); // Use the provided DbContext
            })
            .AddServer(option => { })
            .AddValidation(option => { });
    }
}

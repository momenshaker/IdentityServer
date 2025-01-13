using App.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Core.DataSeed;

/// <summary>
/// A hosted service that seeds initial data for the development and production environments.
/// It ensures that the database is created and migrated, and configures OpenIddict applications for authentication.
/// </summary>
public class DevelopmentDataSeedHostedService<TContext> : IHostedService
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevelopmentDataSeedHostedService<TContext>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevelopmentDataSeedHostedService{TContext}"/> class.
    /// </summary>
    public DevelopmentDataSeedHostedService(
        IServiceProvider serviceProvider,
        IWebHostEnvironment webHostEnvironment,
        IConfiguration configuration,
        ILogger<DevelopmentDataSeedHostedService<TContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Starts the data seeding process. It ensures the database is created and migrated, 
    /// and seeds OpenIddict applications based on the environment (Development or Production).
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();
        var manager = serviceScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        try
        {
            // Ensure the database is created and migrated
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database created and migrated successfully.");

            var config = _configuration.GetSection("ClientSecerts").Get<ClientSecerts>();

            if (_webHostEnvironment.IsDevelopment())
            {
                await SeedDevelopmentData(manager);
            }
            else if (config is not null)
            {
                await SeedProductionData(manager, config!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the data seeding process.");
            throw;  // Re-throw the exception to indicate failure during the start process
        }
    }

    /// <summary>
    /// Seeds the development environment with a default OpenIddict application if not already present.
    /// </summary>
    private async Task SeedDevelopmentData(IOpenIddictApplicationManager manager)
    {
        try
        {
            if (await manager.FindByClientIdAsync("dev_client") is null)
            {
                var application = new OpenIddictApplicationDescriptor
                {
                    ClientId = "dev_client",
                    ClientSecret = "5A80C0B3-8FCE-4B46-A22C-934BDC9EC566",
                    DisplayName = "For development only",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.Password,
                        Permissions.Prefixes.GrantType + "otp_code"
                    }
                };

                await manager.CreateAsync(application);
                _logger.LogInformation("Development OpenIddict application seeded successfully.");
            }
            else
            {
                _logger.LogInformation("Development OpenIddict application already exists.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding development data.");
            throw;
        }
    }

    /// <summary>
    /// Seeds the production environment with a client configuration from the settings.
    /// </summary>
    private async Task SeedProductionData(IOpenIddictApplicationManager manager, ClientSecerts config)
    {
        try
        {
            if (!string.IsNullOrEmpty(config.ClientId))
            {
                if (await manager.FindByClientIdAsync(config.ClientId) is null)
                {
                    var application = new OpenIddictApplicationDescriptor
                    {
                        ClientId = config.ClientId,
                        ClientSecret = config.ClientSecert,
                        DisplayName = "For Production",
                        Permissions =
                        {
                            Permissions.Endpoints.Token,
                            Permissions.GrantTypes.ClientCredentials,
                            Permissions.GrantTypes.Password,
                            Permissions.Prefixes.GrantType + "otp_code"
                        }
                    };

                    await manager.CreateAsync(application);
                    _logger.LogInformation("Production OpenIddict application seeded successfully.");
                }
                else
                {
                    _logger.LogInformation("Production OpenIddict application already exists.");
                }
            }
            else
            {
                _logger.LogWarning("Production client ID is not configured.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding production data.");
            throw;
        }
    }

    /// <summary>
    /// Stops the hosted service (no-op).
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

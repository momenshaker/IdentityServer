using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace App.Services.Options;

/// <summary>
/// Configures the OpenIddict validation builder options for the application.
/// This setup includes enabling the local server, integrating with ASP.NET Core,
/// and using data protection for token validation.
/// </summary>
public class OpenIddictValidationBuilderSetup : IConfigureOptions<OpenIddictValidationBuilder>
{
    /// <summary>
    /// Configures the OpenIddict validation builder options, including the local server,
    /// ASP.NET Core integration, and data protection for token validation.
    /// </summary>
    /// <param name="options">The OpenIddict validation builder options to configure.</param>
    public void Configure(OpenIddictValidationBuilder options)
    {
        // Enable the local OpenIddict server for validation
        options.UseLocalServer();

        // Enable integration with ASP.NET Core
        options.UseAspNetCore();

        // Use data protection for securing tokens
        options.UseDataProtection();
    }
}

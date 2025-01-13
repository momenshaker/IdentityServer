using App.Services.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace App.Services.Options;

/// <summary>
/// Configures the OpenIddict server builder options for the application.
/// This setup includes issuer configuration, token endpoint settings,
/// flow options, token lifetimes, and scope registration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OpenIddictServerBuilderSetup"/> class.
/// </remarks>
/// <param name="configuration">The configuration used to retrieve application settings.</param>
public class OpenIddictServerBuilderSetup(IConfiguration configuration) : IConfigureOptions<OpenIddictServerBuilder>
{
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Configures the OpenIddict server builder options, including the issuer URI, token endpoint URIs,
    /// supported authorization flows, token lifetimes, and the required scopes for the application.
    /// </summary>
    /// <param name="options">The OpenIddict server builder options to configure.</param>
    public void Configure(OpenIddictServerBuilder options)
    {
        // Set the issuer URI for the OpenIddict server
        options.SetIssuer(new Uri(_configuration!.GetValue<string>("Url:ServerSiteUrl")!));

        // Configure data protection and preferred token formats
        options.UseDataProtection()
            .PreferDefaultAccessTokenFormat()
            .PreferDefaultAuthorizationCodeFormat()
            .PreferDefaultDeviceCodeFormat()
            .PreferDefaultRefreshTokenFormat()
            .PreferDefaultUserCodeFormat();

        // Set the token and userinfo endpoint URIs
        options.SetTokenEndpointUris("connect/token");
        options.SetUserinfoEndpointUris("/connect/userinfo");

        // Enable various OAuth2.0 flows
        options.AllowClientCredentialsFlow();
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        options.AllowCustomFlow("otp_code");

        // Add certificates for signing and encryption
        options.AddCertificates(_configuration);

        // Disable access token encryption
        options.DisableAccessTokenEncryption();

        // Set token lifetimes
        options.SetIdentityTokenLifetime(TimeSpan.FromDays(1));
        options.SetAccessTokenLifetime(TimeSpan.FromDays(1));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

        // Register the necessary OpenIddict scopes
        options.RegisterScopes(
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles
        );

        // Enable ASP.NET Core integration and token endpoint passthrough
        options.UseAspNetCore().EnableTokenEndpointPassthrough();
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;

namespace App.Services.Options;

/// <summary>
/// Configures the default authorization policy for the application.
/// This policy ensures that only authenticated users can access resources
/// and supports both JWT Bearer tokens and OpenIddict validation.
/// </summary>
public class AuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    /// <summary>
    /// Configures the authorization options by setting the default authorization policy.
    /// The policy requires users to be authenticated and supports both JWT Bearer and
    /// OpenIddict authentication schemes.
    /// </summary>
    /// <param name="options">The authorization options to configure.</param>
    public void Configure(AuthorizationOptions options)
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    }
}

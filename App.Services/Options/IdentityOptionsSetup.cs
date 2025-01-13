using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace App.Services.Options;

/// <summary>
/// Configures the identity options for the application, including password policy
/// and claims identity settings.
/// </summary>
public class IdentityOptionsSetup : IConfigureOptions<IdentityOptions>
{
    /// <summary>
    /// Configures the identity options by setting password policy and claims identity settings.
    /// The password policy requires digits, lowercase, uppercase, and non-alphanumeric characters,
    /// and a minimum length of 6 characters. It also sets the claim types for username, user ID, and roles.
    /// </summary>
    /// <param name="options">The identity options to configure.</param>
    public void Configure(IdentityOptions options)
    {
        // Password policy settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 0;

        // Claims identity settings
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    }
}

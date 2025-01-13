using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.Services.Options;

/// <summary>
/// Configures the JWT Bearer authentication options for the application.
/// This includes setting the authority URL, token validation parameters, and error handling.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JwtBearerOptionsSetup"/> class.
/// </remarks>
/// <param name="configuration">The configuration used to retrieve application settings.</param>
public class JwtBearerOptionsSetup(IConfiguration configuration) : IConfigureOptions<JwtBearerOptions>
{
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Configures the JWT Bearer options by setting the authority, token validation parameters,
    /// and error details settings for JWT authentication.
    /// </summary>
    /// <param name="options">The JWT Bearer options to configure.</param>
    public void Configure(JwtBearerOptions options)
    {
        // Enable error details in response
        options.IncludeErrorDetails = true;

        // Set the authority URL for token validation (usually the issuer)
        options.Authority = _configuration.GetValue<string>("Url:ServerSiteUrl");

        // Require HTTPS for metadata and token validation
        options.RequireHttpsMetadata = true;

        // Set token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validate the token issuer
            ValidateAudience = false, // Skip audience validation
            ValidateLifetime = true, // Validate the token's expiration time
            ValidateIssuerSigningKey = true // Validate the signing key of the token
        };
    }
}

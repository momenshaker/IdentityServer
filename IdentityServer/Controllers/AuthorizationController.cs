using App.Core.Context;
using App.Core.Data.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityServer.Controllers
{
    [ApiController]
    [Route("connect")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ApplicationDbContext _applicationDbContext;

        public AuthorizationController(
            SignInManager<AspNetUser> signInManager,
            UserManager<AspNetUser> userManager,
            IOpenIddictApplicationManager applicationManager,
            ApplicationDbContext applicationDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _applicationManager = applicationManager;
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// Exchange endpoint for handling token requests
        /// </summary>
        [HttpPost("token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            // OTP Code Grant Type
            if (request.GrantType == "otp_code")
            {
                return await HandleOtpCodeGrantType(request);
            }

            // Password Grant Type
            if (request.IsPasswordGrantType())
            {
                return await HandlePasswordGrantType(request);
            }

            // Client Credentials Grant Type
            if (request.IsClientCredentialsGrantType())
            {
                return await HandleClientCredentialsGrantType(request);
            }

            // Refresh Token Grant Type
            if (request.IsRefreshTokenGrantType())
            {
                return await HandleRefreshTokenGrantType();
            }

            // Invalid Grant Type
            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        /// <summary>
        /// Handle OTP Code Grant Type
        /// </summary>
        private async Task<IActionResult> HandleOtpCodeGrantType(OpenIddictRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId)
                             ?? throw new InvalidOperationException("The application cannot be found.");

            if (await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, request.Code))
            {
                var identity = await CreateClaimsIdentity(user);
                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return ForbidWithError(Errors.InvalidGrant, "The OTP code is invalid.");
        }

        /// <summary>
        /// Handle Password Grant Type
        /// </summary>
        private async Task<IActionResult> HandlePasswordGrantType(OpenIddictRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return ForbidWithError(Errors.InvalidGrant, "The username/password combination is invalid.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return ForbidWithError(Errors.InvalidGrant, "The username/password combination is invalid.");
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
          ///TODO MAILING SERVICE

            return Ok("OTP Code has been sent.");
        }

        /// <summary>
        /// Handle Client Credentials Grant Type
        /// </summary>
        private async Task<IActionResult> HandleClientCredentialsGrantType(OpenIddictRequest request)
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId)
                             ?? throw new InvalidOperationException("The application cannot be found.");

            var identity = await CreateClaimsIdentityForApplication(application);
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handle Refresh Token Grant Type
        /// </summary>
        private async Task<IActionResult> HandleRefreshTokenGrantType()
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            return SignIn(new ClaimsPrincipal(claimsPrincipal), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Create a ClaimsIdentity for the user
        /// </summary>
        private async Task<ClaimsIdentity> CreateClaimsIdentity(AspNetUser user)
        {
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);


            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaims(Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

            identity.SetScopes(new[]
            {
                Scopes.OpenId,
                Scopes.Email,
                Scopes.Profile,
                Scopes.Roles,
                Scopes.OfflineAccess
            });

            identity.SetDestinations(GetDestinations);

            return identity;
        }

        /// <summary>
        /// Create a ClaimsIdentity for the application in client credentials flow
        /// </summary>
        private async Task<ClaimsIdentity> CreateClaimsIdentityForApplication(object application)
        {
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
            identity.SetClaim("app-id", await _applicationManager.GetClientIdAsync(application));
            identity.SetClaim("client-id", await _applicationManager.GetClientIdAsync(application));
            identity.SetDestinations(GetDestinations);

            return identity;
        }

        /// <summary>
        /// Forbid the request with a specific error and description
        /// </summary>
        private IActionResult ForbidWithError(string error, string errorDescription)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = errorDescription
            });

            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Determine the destinations for a claim
        /// </summary>
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;
                    break;

                case Claims.Email:
                    yield return Destinations.AccessToken;
                    if (claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;
                    break;

                case Claims.Role:
                    yield return Destinations.AccessToken;
                    if (claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;
                    break;

                default:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    break;
            }
        }
    }
}

using App.Core.Context;
using App.Core.Data.Models;
using App.Core.DataSeed;
using App.Services.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace App.Services.Extensions
{
    public static class ServiceCollectionExtensions
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
            // Configure AutoMapper
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            // Configure Identity services
            services.AddIdentity<AspNetUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserManager<UserManager<AspNetUser>>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

            // Configure Identity options
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            });

            // Configure Authentication with JWT Bearer
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;
                    options.Authority = configuration.GetValue<string>("Url:ServerSiteUrl");
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            // Configure Authorization policies
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Configure OpenIddict services
            return services.AddOpenIddict()
                .AddCore(builder =>
                {
                    builder.UseEntityFrameworkCore().UseDbContext<TContext>();
                })
                .AddServer(builder =>
                {
                    builder.SetIssuer(new Uri(configuration.GetValue<string>("Url:ServerSiteUrl")));
                    builder.UseDataProtection()
                        .PreferDefaultAccessTokenFormat()
                        .PreferDefaultAuthorizationCodeFormat()
                        .PreferDefaultDeviceCodeFormat()
                        .PreferDefaultRefreshTokenFormat()
                        .PreferDefaultUserCodeFormat();

                    builder.SetTokenEndpointUris("connect/token");
                    builder.SetUserinfoEndpointUris("/connect/userinfo");

                    builder.AllowClientCredentialsFlow();
                    builder.AllowPasswordFlow();
                    builder.AllowRefreshTokenFlow();
                    builder.AllowCustomFlow("otp_code");

                    builder.AddCertificates(configuration);
                    builder.DisableAccessTokenEncryption();
                    builder.SetIdentityTokenLifetime(TimeSpan.FromDays(1));
                    builder.SetAccessTokenLifetime(TimeSpan.FromDays(1));
                    builder.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    builder.RegisterScopes(
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles
                    );

                    builder.UseAspNetCore().EnableTokenEndpointPassthrough();
                })
                .AddValidation(builder =>
                {
                    builder.UseLocalServer();
                    builder.UseAspNetCore();
                    builder.UseDataProtection();
                });
        }

        /// <summary>
        /// Adds development data seed service for OpenId data.
        /// </summary>
        /// <typeparam name="TContext">The type of DbContext.</typeparam>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The updated IServiceCollection with the development data seed service.</returns>
        public static IServiceCollection AddOpenIdDevelopmentDataSeed<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            return services.AddHostedService<DevelopmentDataSeedHostedService<TContext>>();
        }
    }
}

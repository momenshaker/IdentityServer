using App.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Core.DataSeed
{
    public class DevelopmentDataSeedHostedService<TContext> : IHostedService
     where TContext : DbContext
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;


        public DevelopmentDataSeedHostedService(IServiceProvider serviceProvider, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            this.serviceProvider = serviceProvider;
            this._webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var serviceScope = serviceProvider.CreateScope();

            var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();
            await context.Database.EnsureCreatedAsync();
            var _config = _configuration.GetSection("ClientSecerts").Get<ClientSecerts>();
            var manager = serviceScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            if (_webHostEnvironment.IsDevelopment())
            {
                if (await manager.FindByClientIdAsync("dev_client") is null)
                {
                    var application = await manager.CreateAsync(new OpenIddictApplicationDescriptor
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
                    });
                     
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_config.ClientId))
                {
                    if (await manager.FindByClientIdAsync(_config.ClientId) is null)
                    {
                        var application = await manager.CreateAsync(new OpenIddictApplicationDescriptor
                        {
                            ClientId = _config.ClientId,
                            ClientSecret = _config.ClientSecert,
                            DisplayName = "For Production",
                            Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.Password,
                        Permissions.Prefixes.GrantType + "otp_code"

                    }
                        });

                        await context.SaveChangesAsync();
                    }
                }

            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

using App.Core.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Core.Context;

/// <summary>
/// Represents the application's database context that extends IdentityDbContext.
/// This context manages Identity-related entities, such as users and roles.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AspNetUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model using the provided <see cref="ModelBuilder"/>.
    /// This method is called during the model creation process.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignoring the IdentityUserLogin entity as it is not required for the application
        modelBuilder.Ignore<IdentityUserLogin<string>>();

        // Example: Renaming Identity table names or adding customizations
        // modelBuilder.Entity<AspNetUser>().ToTable("CustomUsers");
        // Add your additional customizations after calling base.OnModelCreating(builder);
    }
}

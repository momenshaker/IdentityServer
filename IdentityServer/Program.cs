using App.Core.Context;
using App.Services.Extensions;
using App.Services.Services;
using IdenitityServer.Middlewares;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Configure database context with connection string and OpenIddict
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Configure SQL Server with retry logic
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());

    // Add OpenIddict support to the DbContext
    options.UseOpenIddict();
});

// Add OpenIddict authentication and services
builder.Services.AddOpenId<ApplicationDbContext>(builder.Configuration);

// Register services for dependency injection
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ApplicationDbContext>();

// Security protocols and identity model logging settings
System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// Register development data seeding service
builder.Services.AddOpenIdDevelopmentDataSeed<ApplicationDbContext>();

var app = builder.Build();

// Configure supported cultures and localization options
var supportedCultures = new CultureInfo[] {
    new CultureInfo("en") { DateTimeFormat = { Calendar = new GregorianCalendar() } },
    new CultureInfo("ar") { DateTimeFormat = { Calendar = new GregorianCalendar() } }
};

var localizationOptions = new RequestLocalizationOptions
{
    SupportedCultures = supportedCultures,
    DefaultRequestCulture = new RequestCulture("en")
};

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom middleware for exception handling
app.UseMiddleware<ExceptionMiddleware>();

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers and health checks endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var scopedServiceProvider = scope.ServiceProvider;
    var context = scopedServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Check and apply pending migrations if any
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await context.Database.MigrateAsync();
    }
}

app.Run();

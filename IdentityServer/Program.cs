using App.Services.Extensions;
using IdenitityServer.Options;
using IdentityServer.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfigureOptions<RequestLocalizationOptions>, RequestLocalizationOptionsSetup>();
builder.Services.AddSingleton<IConfigureOptions<MvcNewtonsoftJsonOptions>, MvcNewtonsoftJsonOptionsSetup>();
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterServices(builder.Configuration);
// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseRequestLocalization();

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

app.Run();

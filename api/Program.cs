using System.Text.Json.Serialization;
using api.data;
using api.data.Entities;
using api.Exceptions;
using api.Extensions;
using api.SignalR;
using api.services;
using api.services.EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

// Enable legacy timestamp behavior for Npgsql/PostgreSQL compatibility
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- SERVICE CONFIGURATION ---

// Add SignalR for real-time features
builder.Services.AddSignalR();

// Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithOrigins("http://localhost:5500", "http://localhost:5259", "http://localhost:4200", "http://localhost:5173")
              .WithExposedHeaders(
                  "X-Pagination-Total-Count",
                  "X-Pagination-Page",
                  "X-Pagination-Page-Size",
                  "X-Pagination-Total-Pages",
                  "X-Pagination-Has-Next",
                  "X-Pagination-Has-Previous"
              );
    });
});

// Configure Controllers and JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Extension methods for clean service registration
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); // Added back for documentation
builder.Services.AddHostedService<CampaignCleanupService>();
builder.Services.AddScoped<api.services.EmailService.IEmailService, api.services.EmailService.EmailService>();

var app = builder.Build();

// --- MIDDLEWARE PIPELINE ---
app.UseCors("AllowClient");

// Custom exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

// Serve static and default files
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Map SignalR Hubs
app.MapHub<PresenceHub>("hubs/presence");

// Handle OpenAPI in development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Fallback for Single Page Application routing
app.MapFallbackToController("Index", "Fallback");

// --- DATABASE INITIALIZATION ---

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<ApplicationContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    // Auto-apply pending migrations
    await context.Database.MigrateAsync();

    // Seed initial data
    await SeedData.SaveSeedData(userManager, roleManager, context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration/seeding");
}

app.Run();

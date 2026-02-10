using Gymunity.APIs.Conventions;
using Gymunity.APIs.Hubs;
using Gymunity.APIs.Middlewares;
using Gymunity.APIs.Services;
using Gymunity.Application.DI;
using Gymunity.Infrastructure.Data.DbExtension;
using Gymunity.Infrastructure.DI;
using Gymunity.Infrastructure.Utilities;
using ITI.Gymunity.FP.APIs.Hubs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

// =========================================================
// WEB APPLICATION BUILDER
// =========================================================
var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. SERVICE REGISTRATION
// =========================================================
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// =========================================================
// 2. MIDDLEWARE CONFIGURATION
// =========================================================
await ConfigureMiddlewareAsync(app);


await app.RunAsync();

// =========================================================
// 3. SERVICE CONFIGURATION HELPERS
// =========================================================

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // --- Controller & JSON Configuration ---
    services.AddControllers()
    .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
         options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     })
    .AddMvcOptions(options =>
    {
        // Enable kebab-case transformation for controller names
        options.Conventions.Add(new RouteTokenTransformerConvention(
            new SlugifyParameterTransformer()));
    });

    // Add routing services with lowercase URLs
    services.AddRouting(options =>
    {
        options.LowercaseUrls = true;               // Converts URLs to lowercase
        options.LowercaseQueryStrings = true;       // Optional: lowercase query strings too
        options.AppendTrailingSlash = false;        // Optional: no trailing slash
    });

    // --- Swagger / OpenAPI Configuration ---
    ConfigureSwagger(services);

    // --- Database & Infrastructure ---
    services.AddDbContextServices(configuration);

    services.AddInfrastructureServices();

    services.AddApplicationServices();

    services.AddAuthenticationServices(builder.Configuration);

    // Register Admin Notification Service for API
    // This allows API controllers to send real-time notifications to admins
    // NOTE: Only register IAdminNotificationService and AdminUserResolverService here
    // The notification HANDLERS (PaymentNotificationService, etc.) are only needed in Admin.MVC
    // where they subscribe to events from business logic services
    services.AddScoped<IAdminNotificationService, AdminNotificationService>();
    services.AddScoped<AdminUserResolverService>();

    // Required for DI scope in background tasks or seeding
    services.AddEndpointsApiExplorer();

    services.AddSignalR();

    // --- CORS Configuration for Development ---
    services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policyBuilder =>
        {
            // Allow specific origins for development
            policyBuilder
                .WithOrigins(
                    "http://localhost:4200",    // Angular default
                    "https://localhost:4200",   // Angular with HTTPS
                    "http://localhost:3000",    // React default
                    "https://localhost:3000"    // React with HTTPS
                )
                .AllowAnyMethod()               // Allow all HTTP methods
                .AllowAnyHeader()               // Allow all headers
                .AllowCredentials()             // Allow credentials (cookies, auth headers)
                .SetPreflightMaxAge(TimeSpan.FromHours(1)); // Cache preflight requests

            // Enable detailed CORS logging for debugging
            policyBuilder.SetIsOriginAllowedToAllowWildcardSubdomains();

            // Staging/Production policy
            options.AddPolicy("StagingPolicy", policyBuilder =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? [];

                policyBuilder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    });

    services.AddMemoryCache();

    // Add health checks
    services.AddHealthChecks();

    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
}

// =========================================================
// 4. MIDDLEWARE PIPELINE HELPERS
// =========================================================
async Task ConfigureMiddlewareAsync(WebApplication app)
{
    // 1. Global Exception Handling
    app.UseMiddleware<ExceptionMiddleware>();

    // 2. Custom error pages
    app.UseStatusCodePagesWithReExecute("/errors/{0}");

    // 3. Swagger for Development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gymunity APIs v1"));
    }

    // 4. HTTPS Redirection (optional for dev, required for prod)
    app.UseHttpsRedirection();

    // 5. Static Files
    app.UseStaticFiles();

    // 6. ROUTING 
    app.UseRouting();

    // 7. CORS - Must come after Routing, before Auth
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevelopmentPolicy");
        app.Logger.LogInformation("Development CORS policy enabled");
    }
    else if (app.Environment.IsStaging())
    {
        app.UseCors("StagingPolicy");
    }
    else
    {
        app.UseCors("ProductionPolicy");
    }

    // 8. Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // 9. Webhook Security Middleware
    app.UseWebhookSecurity();

    // 10. Map SignalR Hubs
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<AdminNotificationHub>("/hubs/admin-notifications");

    // 11. Health Check Endpoint
    app.MapHealthChecks("/health").AllowAnonymous();

    // 12. Map Controllers
    app.MapControllers();

    // 13. Database Seeding (last step)
    await app.SeedDatabaseAsync();
}
void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Gymunity APIs",
            Version = "v1",
            Description = "API endpoints for Gymunity application"
        });

        // XML Comments
        var xmlFile = Path.Combine(AppContext.BaseDirectory, "Gymunity.APIs.xml");
        if (File.Exists(xmlFile))
        {
            options.IncludeXmlComments(xmlFile);
        }

        // JWT Security Definition
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token here. Example: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });
}
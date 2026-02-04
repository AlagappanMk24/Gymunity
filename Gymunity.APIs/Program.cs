using Gymunity.APIs.Conventions;
using Gymunity.APIs.Hubs;
using Gymunity.APIs.Middlewares;
using Gymunity.APIs.Services;
using Gymunity.Application.DI;
using Gymunity.Infrastructure.Data.DbExtension;
using Gymunity.Infrastructure.DI;
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

    // Add CORS for SignalR
    services.AddCors(options =>
    {
        options.AddPolicy("wepPolicy", policyBuilder =>
        {
            policyBuilder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(origin => true);
        });
    });

    services.AddMemoryCache();
}

// =========================================================
// 4. MIDDLEWARE PIPELINE HELPERS
// =========================================================
async Task ConfigureMiddlewareAsync(WebApplication app)
{
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseStatusCodePagesWithReExecute("/errors/{0}");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gymunity APIs v1"));
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Map SignalR Hubs
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<AdminNotificationHub>("/hubs/admin-notifications");

    app.UseRouting();
    app.UseCors("wepPolicy");

    app.UseAuthentication();
    app.UseWebhookSecurity();

    await app.SeedDatabaseAsync();

    app.MapControllers();
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
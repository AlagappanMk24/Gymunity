using Gymunity.Application.DI;
using Gymunity.Infrastructure.Data.DbExtension;
using Gymunity.Infrastructure.DI;
using KS_Sweets.Infrastructure.Data.Initializers;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

// =========================================================
// 1. WEB APPLICATION BUILDER
// =========================================================
var builder = WebApplication.CreateBuilder(args);

// Group all service registrations
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// =========================================================
// 2. MIDDLEWARE CONFIGURATION
// =========================================================
ConfigureMiddleware(app);

// Data Seeding
await app.SeedDatabaseAsync();

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
         });

    // --- Swagger / OpenAPI Configuration ---
    ConfigureSwagger(services);

    // --- Database & Infrastructure ---
    services.AddDbContextServices(configuration);
    services.AddApplicationServices(builder.Configuration);
    services.AddInfrastructureServices();

    // Required for DI scope in background tasks or seeding
    services.AddEndpointsApiExplorer();
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gymunity APIs", Version = "v1" });

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

// =========================================================
// 4. MIDDLEWARE PIPELINE HELPERS
// =========================================================
void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gymunity APIs v1"));
    }

    app.UseHttpsRedirection();

    // Routing must come before Auth
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}
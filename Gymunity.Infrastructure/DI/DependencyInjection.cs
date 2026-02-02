using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Auth;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.Services;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Interfaces;
using Gymunity.Domain.Interfaces.Client;
using Gymunity.Infrastructure.Data.Context;
using Gymunity.Infrastructure.Data.Initializers;
using Gymunity.Infrastructure.ExternalServices;
using Gymunity.Infrastructure.Repositories;
using Gymunity.Infrastructure.Repositories.Client;
using Gymunity.Infrastructure.Repositories.Trainer;
using Gymunity.Infrastructure.Services;
using Gymunity.Infrastructure.Services.ExternalAuth.Google;
using Gymunity.Infrastructure.Services.Identity;
using ITI.Gymunity.FP.Infrastructure.Repositories;
using KS_Sweets.Infrastructure.Data.Initializers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gymunity.Infrastructure.DI
{
    public static class DependencyInjection
    {
        // ===========================
        // Data Access & Identity Configuration
        // ===========================
        public static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Configure Context Services
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("GymunityDbConnection"));
            });

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>();

            return services;
        }

        // ===========================
        // Security & Token Configuration
        // ===========================
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration _configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:AuthKey"] ?? string.Empty)),
                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.Zero,
                };
                // ADD THIS: Configure events to return 401 instead of 404
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        // Handle challenge (no token or invalid token)
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            StatusCode = 401,
                            Message = "Unauthorized. Please provide a valid token."
                        };

                        return context.Response.WriteAsJsonAsync(response);
                    },
                    OnForbidden = context =>
                    {
                        // Handle forbidden (valid token but insufficient permissions)
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            StatusCode = 403,
                            Message = "Forbidden. You don't have permission to access this resource."
                        };

                        return context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

            return services;
        }

        // ===========================
        // Repository & Infrastructure Services
        // ===========================
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Core Persistence
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            // Register Client Repositories 
            services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
            services.AddScoped<IReviewClientRepository, ReviewClientRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IProgramRepository, ProgramRepository>();
            services.AddScoped<IReviewAdminRepository, ReviewAdminRepository>();
            services.AddScoped<IWorkoutLogRepository, WorkoutLogRepository>();

            // Register Trainer Repositories 
            services.AddScoped<ITrainerProfileRepository, TrainerProfileRepository>();

            // Identity & Auth Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IPasswordService, PasswordService>();

            // Communication & Utility Services
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IImageUrlResolver, ImageUrlResolver>();
            services.AddScoped<INotificationService, NotificationService>();

            // Payment Gateway Services
            services.AddScoped<IStripePaymentService, StripePaymentService>();
            services.AddScoped<IPayPalService, PayPalService>();
            return services;
        }
    }
}
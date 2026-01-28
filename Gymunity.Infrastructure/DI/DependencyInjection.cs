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
using Gymunity.Infrastructure.Data.Context;
using Gymunity.Infrastructure.Data.Initializers;
using Gymunity.Infrastructure.ExternalServices;
using Gymunity.Infrastructure.Repositories;
using Gymunity.Infrastructure.Services;
using Gymunity.Infrastructure.Services.ExternalAuth.Google;
using Gymunity.Infrastructure.Services.Identity;
using ITI.Gymunity.FP.Infrastructure.Repositories;
using KS_Sweets.Infrastructure.Data.Initializers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration _configuration)
        {

            // Adding Authintication Schema Bearer
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

            });

            return services;
        }
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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            // Register Repositories 
            services.AddScoped<IProgramRepository, ProgramRepository>();
  
            // Register External Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IImageUrlResolver, ImageUrlResolver>();

            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IReviewAdminRepository, ReviewAdminRepository>();
            return services;
        }
    }
}
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Contracts.Services.Packages;
using Gymunity.Application.Mapping;
using Gymunity.Application.Services.Admin;
using Gymunity.Application.Services.Packages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gymunity.Application.DI
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddApplicationServices( this IServiceCollection services, IConfiguration configuration)
        {

            // ===========================
            // AutoMapper
            // ===========================
            services.AddAutoMapper((opt) => { }, typeof(MappingProfile).Assembly);

            // ===========================
            // Admin Services
            // ===========================
            services.AddScoped<IClientAdminService, ClientAdminService>();
            services.AddScoped<ITrainerAdminService, TrainerAdminService>();
            services.AddScoped<IPackageService, PackageService>();

            return services;
        }
    }
}
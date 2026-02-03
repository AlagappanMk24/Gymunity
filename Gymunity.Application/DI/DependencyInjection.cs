using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.Contracts.Services.Packages;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.Mapping;
using Gymunity.Application.Services.Admin;
using Gymunity.Application.Services.Client;
using Gymunity.Application.Services.Packages;
using Gymunity.Application.Services.Trainer;
using Microsoft.Extensions.DependencyInjection;

namespace Gymunity.Application.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            // ===========================
            // Core Tools & Mapping (AutoMapper)
            // ===========================
            services.AddAutoMapper((opt) => { }, typeof(MappingProfile).Assembly);

            services.AddHttpClient();
            // ===========================
            // Admin Feature Services
            // ===========================
            services.AddScoped<IClientAdminService, ClientAdminService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IPaymentAdminService, PaymentAdminService>();
            services.AddScoped<IProgramAdminService, ProgramAdminService>();
            services.AddScoped<IReviewAdminService, ReviewAdminService>();
            services.AddScoped<ISubscriptionAdminService, SubscriptionAdminService>();
            services.AddScoped<ITrainerAdminService, TrainerAdminService>();

            // ===========================
            // Client Feature Services
            // ===========================
            services.AddScoped<IClientProfileService, ClientProfileService>();
            services.AddScoped<IOnboardingService, OnboardingService>();
            services.AddScoped<IClientProgramsService, ClientProgramsService>();
            services.AddScoped<IWorkoutLogService, WorkoutLogService>();
            services.AddScoped<IBodyStateLogService, BodyStateLogService>();
            services.AddScoped<IClientTrainersService, ClientTrainersService>();
            services.AddScoped<IReviewClientService, ReviewClientService>();
            services.AddScoped<IPaymentService, PaymentService>();


            // ===========================
            // Trainer Feature Services
            // ===========================
            services.AddScoped<ITrainerProfileService, TrainerProfileService>();
            services.AddScoped<ITrainerProgramService, TrainerProgramService>();
            services.AddScoped<IWeekService, WeekService>();
            services.AddScoped<IDayService, DayService>();
            services.AddScoped<IDayExerciseService, DayExerciseService>();
            services.AddScoped<IExerciseLibraryService, ExerciseLibraryService>();

            return services;
        }
    }
}
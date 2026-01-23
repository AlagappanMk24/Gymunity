using Gymunity.Admin.MVC.Hubs;
using Gymunity.Admin.MVC.Services;
using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DI;
using Gymunity.Application.Services.Admin;
using Gymunity.Infrastructure.DI;
using Gymunity.Infrastructure.Services.Identity;

namespace Gymunity.Admin.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add SignalR
            builder.Services.AddSignalR();

            // Add CORS for SignalR
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("adminSignalRPolicy", policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(origin => true);
                });
            });

            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", config =>
                {
                    config.Cookie.Name = "Gymunity.Admin.Cookie";
                    config.LoginPath = "/Auth/Login";
                });
            builder.Services.AddDbContextServices(builder.Configuration);
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddInfrastructureServices();

            // Add Dashboard Service
            builder.Services.AddScoped<IDashboardStatisticsService, DashboardStatisticsService>();


            // ✅ Register Admin Services (required for notification handlers)
            builder.Services.AddScoped<ITrainerAdminService, TrainerAdminService>();
            
            // ✅ Register AccountService (from Infrastructure layer)
            builder.Services.AddScoped<IAccountService, AccountService>();

            // Add Admin Notification Services
            builder.Services.AddScoped<IAdminUserResolverService, AdminUserResolverService>();
            builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable CORS before mapping SignalR hubs
            app.UseCors("adminSignalRPolicy");

            // Map SignalR hub
            app.MapHub<AdminNotificationHub>("/hubs/admin-notifications");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}

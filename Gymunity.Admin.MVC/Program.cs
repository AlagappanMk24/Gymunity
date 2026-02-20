using Gymunity.Admin.MVC.Hubs;
using Gymunity.Admin.MVC.Services;
using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DI;
using Gymunity.Application.Services.Admin;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Infrastructure.Data.Context;
using Gymunity.Infrastructure.DI;
using Gymunity.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
                    if (builder.Environment.IsDevelopment())
                    {
                        policyBuilder
                            .WithOrigins("https://localhost:4200", "http://localhost:4200", "https://localhost:7182")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                    else
                    {
                        policyBuilder
                            .WithOrigins("https://localhost:7182")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                });
            });

            //Configure Context Services
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("GymunityDbConnection"));
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Configure Identity with cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // The name of the cookie stored in the user's browser (useful for avoiding conflicts if you have multiple apps on the same domain)
                options.Cookie.Name = "Gymunity.Admin.Cookie";

                // Prevents client-side scripts (like JavaScript) from accessing the cookie, which mitigates Cross-Site Scripting (XSS) attacks
                options.Cookie.HttpOnly = true;

                // 'Lax' allows the cookie to be sent on top-level navigations (like clicking a link from another site), balancing security and user experience
                options.Cookie.SameSite = SameSiteMode.Lax;

                // Forces the cookie to only be transmitted over HTTPS connections, protecting it from being intercepted over plain HTTP
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                // Marks the cookie as vital for the app to function; it will bypass user consent checks in some GDPR/privacy-compliance scenarios
                options.Cookie.IsEssential = true;

                // Sets the total lifespan of the authentication session; in this case, the user stays logged in for 7 days
                //options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1);

                //// If true, the expiration time resets every time the user visits the site within the window, keeping active users logged in indefinitely
                //options.SlidingExpiration = true;

                // The URL path the browser redirects to if a user tries to access a protected page while not logged in
                options.LoginPath = "/auth/login";

                // The URL path used to handle the sign-out process
                options.LogoutPath = "/auth/logout";

                // The URL path the user is sent to if they are logged in but do not have the required Role or Policy to view a specific page
                options.AccessDeniedPath = "/auth/access-denied";
            });

            //builder.Services.AddDbContextServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices();

            // Add Dashboard Service
            builder.Services.AddScoped<IDashboardStatisticsService, DashboardStatisticsService>();

            // Add Analytics Service
            builder.Services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();

            // ✅ Register Admin Services (required for notification handlers)
            builder.Services.AddScoped<ITrainerAdminService, TrainerAdminService>();
            builder.Services.AddScoped<ISubscriptionAdminService, SubscriptionAdminService>();
            builder.Services.AddScoped<IPaymentAdminService, PaymentAdminService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();

            // ✅ Register AccountService (from Infrastructure layer)
            builder.Services.AddScoped<IAccountService, AccountService>();

            // Add Admin Notification Services
            builder.Services.AddScoped<IAdminUserResolverService, AdminUserResolverService>();
            builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();

            // ✅ Register All Admin Notification Handlers
            // These services subscribe to events from business logic services
            // and send real-time notifications to admins via SignalR
            builder.Services.AddScoped<IAccountNotificationService, AccountNotificationService>();      // User registrations
            builder.Services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();      // Payment events
            builder.Services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>(); // Subscription events
            builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();         // User management events
            builder.Services.AddScoped<ITrainerNotificationService, TrainerNotificationService>();      // Trainer management events

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable CORS before mapping SignalR hubs
            app.UseCors("adminSignalRPolicy");

            // Map SignalR hub
            app.MapHub<AdminNotificationHub>("/hubs/admin-notifications");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=auth}/{action=login}/{id?}");

            app.Run();
        }
    }
}
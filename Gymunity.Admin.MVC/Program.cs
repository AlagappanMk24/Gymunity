using Gymunity.Admin.MVC.Services;
using Gymunity.Application.DI;
using Gymunity.Infrastructure.DI;

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
            builder.Services.AddScoped<DashboardStatisticsService>();
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
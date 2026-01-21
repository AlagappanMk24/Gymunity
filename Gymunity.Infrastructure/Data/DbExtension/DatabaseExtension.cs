using KS_Sweets.Infrastructure.Data.Initializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gymunity.Infrastructure.Data.DbExtension
{
    public static class DatabaseExtension
    {
        public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var initializer = services.GetRequiredService<IDbInitializer>();
                await initializer.InitializeAsync();
            }
            catch (Exception ex)
            {
                // Pass the class name as a string category
                var logger = services.GetRequiredService<ILoggerFactory>()
                                     .CreateLogger("DatabaseExtension");

                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
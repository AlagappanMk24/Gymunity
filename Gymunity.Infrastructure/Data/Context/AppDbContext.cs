using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Messaging;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Gymunity.Infrastructure.Data.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
    {
        // DbSet properties for entities
        #region DbSets

        // User & Profiles
        public DbSet<TrainerProfile> TrainerProfiles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<TrainerReview> TrainerReviews { get; set; }

        // Programs
        public DbSet<Program> Programs { get; set; }
        public DbSet<ProgramWeek> ProgramWeeks { get; set; }
        public DbSet<ProgramDay> ProgramDays { get; set; }
        public DbSet<ProgramDayExercise> ProgramDayExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }


        // Packages & Subscriptions
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageProgram> PackagePrograms { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }


        // Client Logs
        public DbSet<WorkoutLog> WorkoutLogs { get; set; }
        public DbSet<BodyStatLog> BodyStatLogs { get; set; }


        // Messaging
        public DbSet<MessageThread> MessageThreads { get; set; }
        public DbSet<Message> Messages { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }

        #endregion

        // Configure the model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the Configurations folder BEFORE setting global filters
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Set global query filter for soft deletes on BaseEntity derivatives
            SetGlobalQueryFilter<BaseEntity>(modelBuilder, e => !e.IsDeleted);
        }
        private static void SetGlobalQueryFilter<TBase>(ModelBuilder modelBuilder, Expression<Func<TBase, bool>> filter) where TBase : class
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Only apply filter to types that inherit from TBase
                if (typeof(TBase).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(TBase))
                {
                    try
                    {
                        var parameter = Expression.Parameter(entityType.ClrType);
                        var body = ReplacingExpressionVisitor.Replace(filter.Parameters[0], parameter, filter.Body);
                        var lambda = Expression.Lambda(body, parameter);

                        entityType.SetQueryFilter(lambda);
                    }
                    catch
                    {
                        // Skip if filter cannot be applied to this entity type
                    }
                }
            }
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
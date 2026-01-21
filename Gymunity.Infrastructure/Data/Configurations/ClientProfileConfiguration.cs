using Gymunity.Domain.Entities.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
    {
        public void Configure(EntityTypeBuilder<ClientProfile> builder)
        {
            // Key Configuration
            builder.HasKey(cp => cp.Id);

            // Property Configurations
            builder.Property(cp => cp.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cp => cp.HeightCm);

            builder.Property(cp => cp.StartingWeightKg)
                .HasPrecision(5, 2);

            builder.Property(cp => cp.Gender)
                .HasMaxLength(50);

            builder.Property(cp => cp.Goal)
                .HasMaxLength(100);

            builder.Property(cp => cp.ExperienceLevel)
                .HasMaxLength(50);

            // Soft Delete
            builder.Property(cp => cp.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(cp => cp.UpdatedAt);

            // Indexes
            builder.HasIndex(cp => cp.UserId)
                .IsUnique();

            // Relationships
            builder.HasOne(cp => cp.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<ClientProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cp => cp.Subscriptions)
                .WithOne()
                .HasForeignKey("ClientProfileId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cp => cp.BodyStatLogs)
                .WithOne(c => c.ClientProfile)
                .HasForeignKey(bs => bs.ClientProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cp => cp.WorkoutLogs)
                .WithOne(c => c.ClientProfile)
                .HasForeignKey(wo => wo.ClientProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
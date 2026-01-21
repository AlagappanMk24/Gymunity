using Gymunity.Domain.Entities.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
    {
        public void Configure(EntityTypeBuilder<WorkoutLog> builder)
        {
            // Table Configuration
            builder.ToTable("WorkoutLogs");

            // Key Configuration - Using long for high volume
            builder.HasKey(wl => wl.Id);

            // Property Configurations
            builder.Property(wl => wl.ClientProfileId)
                .IsRequired();

            builder.Property(wl => wl.ProgramDayId)
                .IsRequired();

            builder.Property(wl => wl.CompletedAt)
                .IsRequired();

            builder.Property(wl => wl.Notes)
                .HasMaxLength(2000);

            builder.Property(wl => wl.DurationMinutes);

            builder.Property(wl => wl.ExercisesLoggedJson)
                .IsRequired()
                .HasMaxLength(8000)
                .HasDefaultValue("[]");

            // Soft Delete
            builder.Property(wl => wl.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(wl => wl.UpdatedAt);

            // Indexes
            builder.HasIndex(wl => wl.ClientProfileId);
            builder.HasIndex(wl => wl.ProgramDayId);
            builder.HasIndex(wl => new { wl.ClientProfileId, wl.CompletedAt });
            builder.HasIndex(wl => wl.CompletedAt);
            builder.HasIndex(wl => new { wl.ClientProfileId, wl.ProgramDayId });


            // Relationships
            builder.HasOne(wl => wl.ClientProfile)
                .WithMany(u => u.WorkoutLogs)
                .HasForeignKey(wl => wl.ClientProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wl => wl.ProgramDay)
                .WithMany()
                .HasForeignKey(wl => wl.ProgramDayId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
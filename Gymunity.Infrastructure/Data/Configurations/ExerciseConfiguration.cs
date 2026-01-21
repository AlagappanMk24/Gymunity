using Gymunity.Domain.Entities.ProgramAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            // Key Configuration
            builder.HasKey(e => e.Id);

            // Property Configurations
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.MuscleGroup)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Equipment)
                .HasMaxLength(100);

            builder.Property(e => e.VideoDemoUrl)
                .HasMaxLength(500);

            builder.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500);

            builder.Property(e => e.IsCustom)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.TrainerId)
                .HasMaxLength(450);

            // Soft Delete
            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(e => e.UpdatedAt);

            // Indexes
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.Category);
            builder.HasIndex(e => e.MuscleGroup);
            builder.HasIndex(e => e.IsCustom);
            builder.HasIndex(e => e.TrainerId);
            builder.HasIndex(e => new { e.IsCustom, e.TrainerId });

            // Relationships
            builder.HasOne(e => e.Trainer)
                .WithMany()
                .HasForeignKey(e => e.TrainerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        }
    }
}
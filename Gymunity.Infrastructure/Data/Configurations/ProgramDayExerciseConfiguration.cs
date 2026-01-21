using Gymunity.Domain.Entities.ProgramAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class ProgramDayExerciseConfiguration : IEntityTypeConfiguration<ProgramDayExercise>
    {
        public void Configure(EntityTypeBuilder<ProgramDayExercise> builder)
        {
            // Table Configuration
            builder.ToTable("ProgramDayExercises");

            // Key Configuration
            builder.HasKey(pde => pde.Id);

            // Property Configurations
            builder.Property(pde => pde.ProgramDayId)
                .IsRequired();

            builder.Property(pde => pde.ExerciseId)
                .IsRequired();

            builder.Property(pde => pde.OrderIndex)
                .IsRequired();

            builder.Property(pde => pde.Sets)
                .HasMaxLength(50);

            builder.Property(pde => pde.Reps)
                .HasMaxLength(50);

            builder.Property(pde => pde.RestSeconds);

            builder.Property(pde => pde.Tempo)
                .HasMaxLength(50);

            builder.Property(pde => pde.RPE)
                .HasPrecision(3, 1);

            builder.Property(pde => pde.Percent1RM)
                .HasPrecision(5, 2);

            builder.Property(pde => pde.Notes)
                .HasMaxLength(1000);

            builder.Property(pde => pde.VideoUrl)
                .HasMaxLength(500);

            builder.Property(pde => pde.ExerciseDataJson)
                .HasMaxLength(4000);

            // Soft Delete
            builder.Property(pde => pde.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(pde => pde.UpdatedAt);

            // Indexes
            builder.HasIndex(pde => pde.ProgramDayId);
            builder.HasIndex(pde => pde.ExerciseId);
            builder.HasIndex(pde => new { pde.ProgramDayId, pde.OrderIndex });

            // Relationships
            builder.HasOne(pde => pde.Day)
                .WithMany(pd => pd.Exercises)
                .HasForeignKey(pde => pde.ProgramDayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pde => pde.Exercise)
                .WithMany()
                .HasForeignKey(pde => pde.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
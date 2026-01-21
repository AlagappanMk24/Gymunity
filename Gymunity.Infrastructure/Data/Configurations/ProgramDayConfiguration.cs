using Gymunity.Domain.Entities.ProgramAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class ProgramDayConfiguration : IEntityTypeConfiguration<ProgramDay>
    {
        public void Configure(EntityTypeBuilder<ProgramDay> builder)
        {
            // Table Configuration
            builder.ToTable("ProgramDays");

            // Key Configuration
            builder.HasKey(pd => pd.Id);

            // Property Configurations
            builder.Property(pd => pd.ProgramWeekId)
                .IsRequired();

            builder.Property(pd => pd.DayNumber)
                .IsRequired();

            builder.Property(pd => pd.Title)
                .HasMaxLength(256);

            builder.Property(pd => pd.Notes)
                .HasMaxLength(1000);

            // Soft Delete
            builder.Property(pd => pd.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(pd => pd.UpdatedAt);

            // Indexes
            builder.HasIndex(pd => pd.ProgramWeekId);
            builder.HasIndex(pd => new { pd.ProgramWeekId, pd.DayNumber });

            // Relationships
            builder.HasOne(pd => pd.Week)
                .WithMany(pw => pw.Days)
                .HasForeignKey(pd => pd.ProgramWeekId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pd => pd.Exercises)
                .WithOne(pde => pde.Day)
                .HasForeignKey(pde => pde.ProgramDayId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
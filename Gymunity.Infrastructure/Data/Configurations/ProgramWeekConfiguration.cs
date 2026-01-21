using Gymunity.Domain.Entities.ProgramAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class ProgramWeekConfiguration : IEntityTypeConfiguration<ProgramWeek>
    {
        public void Configure(EntityTypeBuilder<ProgramWeek> builder)
        {
            // Table Configuration
            builder.ToTable("ProgramWeeks");

            // Key Configuration
            builder.HasKey(pw => pw.Id);

            // Property Configurations
            builder.Property(pw => pw.ProgramId)
                .IsRequired();

            builder.Property(pw => pw.WeekNumber)
                .IsRequired();

            // Soft Delete
            builder.Property(pw => pw.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(pw => pw.ProgramId);
            builder.HasIndex(pw => new { pw.ProgramId, pw.WeekNumber });

            // Relationships
            builder.HasOne(pw => pw.Program)
                .WithMany(p => p.Weeks)
                .HasForeignKey(pw => pw.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pw => pw.Days)
                .WithOne(pd => pd.Week)
                .HasForeignKey(pd => pd.ProgramWeekId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
using Gymunity.Domain.Entities.Trainer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class PackageProgramConfiguration : IEntityTypeConfiguration<PackageProgram>
    {
        public void Configure(EntityTypeBuilder<PackageProgram> builder)
        {
            // Key Configuration
            builder.HasKey(pp => pp.Id);

            // Property Configurations
            builder.Property(pp => pp.PackageId)
                .IsRequired();

            builder.Property(pp => pp.ProgramId)
                .IsRequired();

            // Soft Delete
            builder.Property(pp => pp.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(pp => pp.UpdatedAt);

            // Indexes
            builder.HasIndex(pp => pp.PackageId);
            builder.HasIndex(pp => pp.ProgramId);
            builder.HasIndex(pp => new { pp.PackageId, pp.ProgramId })
                .IsUnique();

            // Relationships
            builder.HasOne(pp => pp.Package)
                .WithMany(p => p.PackagePrograms)
                .HasForeignKey(pp => pp.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pp => pp.Program)
                .WithMany(p => p.PackagePrograms)
                .HasForeignKey(pp => pp.ProgramId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
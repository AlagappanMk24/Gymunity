using Gymunity.Domain.Entities.ProgramAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class ProgramConfiguration : IEntityTypeConfiguration<Program>
    {
        public void Configure(EntityTypeBuilder<Program> builder)
        {
            // Table Configuration
            builder.ToTable("Programs");

            // Key Configuration
            builder.HasKey(p => p.Id);


            // Property Configurations
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.Description)
                .HasMaxLength(2000)
                .HasDefaultValue(string.Empty);

            builder.Property(p => p.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.DurationWeeks)
                .IsRequired();

            builder.Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Property(p => p.IsPublic)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.MaxClients);

            builder.Property(p => p.ThumbnailUrl)
                .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasDefaultValue(new DateTimeOffset());

            // Soft Delete
            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(p => p.TrainerProfileId);
            builder.HasIndex(p => p.TrainerId);
            builder.HasIndex(p => p.IsPublic);
            builder.HasIndex(p => new { p.TrainerProfileId, p.IsPublic });
            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => p.Title).IsUnique();

            // Relationships
            builder.HasOne(p => p.TrainerProfile)
                .WithMany(tp => tp.Programs)
                .HasForeignKey(p => p.TrainerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Weeks)
                .WithOne(w => w.Program)
                .HasForeignKey(w => w.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.PackagePrograms)
                .WithOne(pp => pp.Program)
                .HasForeignKey(pp => pp.ProgramId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
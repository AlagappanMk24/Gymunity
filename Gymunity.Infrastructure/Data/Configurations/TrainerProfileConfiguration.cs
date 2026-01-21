using Gymunity.Domain.Entities.Trainer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class TrainerProfileConfiguration : IEntityTypeConfiguration<TrainerProfile>
    {
        public void Configure(EntityTypeBuilder<TrainerProfile> builder)
        {
            // Table Configuration
            builder.ToTable("TrainerProfiles");

            // Key Configuration
            builder.HasKey(tp => tp.Id);

            // Property Configurations
            builder.Property(tp => tp.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(tp => tp.Handle)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(tp => tp.Bio)
                .HasMaxLength(1000)
                .HasDefaultValue(string.Empty);

            builder.Property(tp => tp.CoverImageUrl)
                .HasMaxLength(500);

            builder.Property(tp => tp.VideoIntroUrl)
                .HasMaxLength(500);

            builder.Property(tp => tp.BrandingColors)
                .HasMaxLength(500);

            builder.Property(tp => tp.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(tp => tp.VerifiedAt);

            builder.Property(tp => tp.RatingAverage)
                .HasPrecision(3, 2)
                .HasDefaultValue(0m);

            builder.Property(tp => tp.TotalClients)
                .HasDefaultValue(0);

            builder.Property(tp => tp.YearsExperience)
                .IsRequired();

            // Soft Delete
            builder.Property(tp => tp.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(tp => tp.UserId)
                .IsUnique();

            builder.HasIndex(tp => tp.Handle)
                .IsUnique();

            builder.HasIndex(tp => tp.IsVerified);

            // Relationships
            builder.HasOne(tp => tp.User)
                .WithOne(u => u.TrainerProfile)
                .HasForeignKey<TrainerProfile>(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
using Gymunity.Domain.Entities.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.Gymunity.FP.Infrastructure._Data.Configurations
{
    public class BodyStatLogConfiguration : IEntityTypeConfiguration<BodyStatLog>
    {
        public void Configure(EntityTypeBuilder<BodyStatLog> builder)
        {
            // Key Configuration
            builder.HasKey(bsl => bsl.Id);

            // Property Configurations
            builder.Property(bsl => bsl.ClientProfileId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(bsl => bsl.LoggedAt)
                .IsRequired();

            builder.Property(bsl => bsl.WeightKg)
                .HasPrecision(5, 2);

            builder.Property(bsl => bsl.BodyFatPercent)
                .HasPrecision(5, 2);

            builder.Property(bsl => bsl.MeasurementsJson)
                .HasMaxLength(1000);

            builder.Property(bsl => bsl.PhotoFrontUrl)
                .HasMaxLength(500);

            builder.Property(bsl => bsl.PhotoSideUrl)
                .HasMaxLength(500);

            builder.Property(bsl => bsl.PhotoBackUrl)
                .HasMaxLength(500);

            builder.Property(bsl => bsl.Notes)
                .HasMaxLength(1000);

            // Soft Delete
            builder.Property(bsl => bsl.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(bsl => bsl.UpdatedAt);

            // Indexes
            builder.HasIndex(bsl => bsl.ClientProfileId);
            builder.HasIndex(bsl => new { bsl.ClientProfileId, bsl.LoggedAt });
            builder.HasIndex(bsl => bsl.LoggedAt);

            // Relationships
            builder.HasOne(bsl => bsl.ClientProfile)
                .WithMany(u => u.BodyStatLogs)
                .HasForeignKey(bsl => bsl.ClientProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
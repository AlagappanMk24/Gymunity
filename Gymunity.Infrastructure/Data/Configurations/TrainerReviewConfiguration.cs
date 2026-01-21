using Gymunity.Domain.Entities.Trainer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    internal class TrainerReviewConfiguration : IEntityTypeConfiguration<TrainerReview>
    {
        public void Configure(EntityTypeBuilder<TrainerReview> builder)
        {
            // Primary Key
            builder.HasKey(r => r.Id);

            // Relationships
            builder.HasOne(r => r.Trainer)
                   .WithMany(t => t.TrainerReviews)
                   .HasForeignKey(r => r.TrainerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Client)
                   .WithMany(c => c.TrainerReviews)
                   .HasForeignKey(r => r.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Unique Constraint: prevent duplicate reviews
            builder.HasIndex(r => new { r.ClientId, r.TrainerId })
                   .IsUnique();

            // Check Constraint: Rating between 1 and 5
            // Obsolete API warning: use ToTable(... => t.HasCheckConstraint(...)) when configuring table.
            builder.HasCheckConstraint("CK_TrainerReviews_Rating", "[Rating] BETWEEN 1 AND 5");

            // Indexes for performance
            builder.HasIndex(r => r.TrainerId);
            builder.HasIndex(r => r.ClientId);

            // Optional: Global Query Filter for Soft Delete
            builder.HasQueryFilter(r => !r.IsDeleted);

            // Additional property configuration
            builder.Property(r => r.Rating)
                   .IsRequired();

            builder.Property(r => r.Comment)
                   .HasMaxLength(1000); // Optional, limit comment size
        }
    }
}
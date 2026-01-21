using Gymunity.Domain.Entities.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class MessageThreadConfiguration : IEntityTypeConfiguration<MessageThread>
    {
        public void Configure(EntityTypeBuilder<MessageThread> builder)
        {
            // Key Configuration
            builder.HasKey(mt => mt.Id);

            // Property Configurations
            builder.Property(mt => mt.ClientId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(mt => mt.TrainerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(mt => mt.LastMessageAt)
                .IsRequired()
                .HasDefaultValue(new DateTime());

            builder.Property(mt => mt.IsPriority)
                .IsRequired()
                .HasDefaultValue(false);

            // Soft Delete
            builder.Property(mt => mt.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(mt => mt.UpdatedAt);

            // Indexes
            //builder.HasIndex(mt => mt.ClientId);
            //builder.HasIndex(mt => mt.TrainerId);
            //builder.HasIndex(mt => new { mt.ClientId, mt.TrainerId })
            //    .IsUnique();
            builder.HasIndex(mt => mt.LastMessageAt);
            builder.HasIndex(mt => mt.IsPriority);

            // Relationships
            builder.HasOne(mt => mt.Client)
                .WithMany()
                .HasForeignKey(mt => mt.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(mt => mt.Trainer)
                .WithMany()
                .HasForeignKey(mt => mt.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(mt => mt.Messages)
                .WithOne(m => m.Thread)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Gymunity.Domain.Entities.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            // Key Configuration - Using long for high volume
            builder.HasKey(m => m.Id);

            // Property Configurations
            builder.Property(m => m.ThreadId)
                .IsRequired();

            builder.Property(m => m.SenderId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(4000)
                .HasDefaultValue(string.Empty);

            builder.Property(m => m.MediaUrl)
                .HasMaxLength(500);

            builder.Property(m => m.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(m => m.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            // Soft Delete
            builder.Property(m => m.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(m => m.UpdatedAt);

            // Indexes
            builder.HasIndex(m => m.ThreadId);
            builder.HasIndex(m => m.SenderId);
            builder.HasIndex(m => m.CreatedAt);
            builder.HasIndex(m => new { m.ThreadId, m.CreatedAt });
            builder.HasIndex(m => new { m.ThreadId, m.IsRead });

            // Relationships
            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Thread)
                .WithMany(mt => mt.Messages)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

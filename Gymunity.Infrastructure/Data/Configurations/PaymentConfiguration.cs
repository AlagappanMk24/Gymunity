using Gymunity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Key Configuration
            builder.HasKey(p => p.Id);

            // Property Configurations
            builder.Property(p => p.SubscriptionId)
                .IsRequired();

            builder.Property(p => p.PaymobTransactionId)
                .HasMaxLength(255);

            builder.Property(p => p.PayPalOrderId)
                .HasMaxLength(255);

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("EGP");

            builder.Property(p => p.PlatformFee)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.TrainerPayout)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<int>()
                .IsRequired();

            // Soft Delete
            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(p => p.UpdatedAt);

            // Indexes
            builder.HasIndex(p => p.SubscriptionId);
            builder.HasIndex(p => p.PaymobTransactionId)
                .IsUnique();
            builder.HasIndex(p => p.PayPalOrderId)
                .IsUnique();
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.PaidAt);

            // Relationships
            builder.HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
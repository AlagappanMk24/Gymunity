using Gymunity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            // Table Configuration
            builder.ToTable("Subscriptions");

            // Key Configuration
            builder.HasKey(s => s.Id);

            // Property Configurations
            builder.Property(s => s.ClientId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(s => s.PackageId)
                .IsRequired();

            builder.Property(s => s.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.CurrentPeriodEnd)
                .IsRequired();

            builder.Property(s => s.PaymobOrderId)
                .HasMaxLength(255);

            builder.Property(s => s.PaymobTransactionId)
                .HasMaxLength(255);

            builder.Property(s => s.PayPalSubscriptionId)
                .HasMaxLength(255);

            builder.Property(s => s.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("EGP");

            builder.Property(s => s.AmountPaid)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(s => s.PlatformFeePercentage)
                .HasPrecision(5, 2)
                .HasDefaultValue(15m);

            builder.Property(s => s.CanceledAt);

            // Soft Delete
            builder.Property(s => s.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(s => s.UpdatedAt);

            // Indexes
            builder.HasIndex(s => s.ClientId);
            builder.HasIndex(s => s.PackageId);
            builder.HasIndex(s => s.Status);
            builder.HasIndex(s => s.PaymobOrderId);
            builder.HasIndex(s => s.PayPalSubscriptionId);
            builder.HasIndex(s => new { s.ClientId, s.Status });

            // Relationships
            builder.HasOne(s => s.Client)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Package)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Payments)
                .WithOne(p => p.Subscription)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
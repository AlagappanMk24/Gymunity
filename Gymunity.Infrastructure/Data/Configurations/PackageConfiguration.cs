using Gymunity.Domain.Entities.Trainer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class PackageConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            // Table Configuration
            builder.ToTable("Packages");

            // Key Configuration
            builder.HasKey(p => p.Id);

            // Property Configurations
            builder.Property(p => p.TrainerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.Description)
                .HasMaxLength(2000)
                .HasDefaultValue(string.Empty);

            builder.Property(p => p.PriceMonthly)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.PriceYearly)
                .HasPrecision(18, 2);

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("EGP");

            builder.Property(p => p.FeaturesJson)
                .IsRequired()
                .HasDefaultValue("{}");

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.ThumbnailUrl)
                .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValue(new DateTime());

            // New properties
            //builder.Property(p => p.IsAnnual)
                //.IsRequired()
                //.HasDefaultValue(false);

            // Ensure PromoCode has a default value to prevent NULL inserts
            builder.Property(p => p.PromoCode)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue(string.Empty);

            // Soft Delete
            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(p => p.UpdatedAt);

            // Indexes
            builder.HasIndex(p => p.TrainerId);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => new { p.TrainerId, p.IsActive });
            builder.HasIndex(p => p.Name).IsUnique();


            // Relationships
            builder.HasOne(p => p.Trainer)
                .WithMany()
                .HasForeignKey(p => p.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.PackagePrograms)
                .WithOne(pp => pp.Package)
                .HasForeignKey(pp => pp.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Subscriptions)
                .WithOne(s => s.Package)
                .HasForeignKey(s => s.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Trainer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymunity.Infrastructure.Data.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Key Configuration
            builder.HasKey(u => u.Id);

            // Property Configurations
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.ProfilePhotoUrl)
                .HasMaxLength(500);

            builder.Property(u => u.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(u => u.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.StripeCustomerId)
                .HasMaxLength(255);

            builder.Property(u => u.StripeConnectAccountId)
                .HasMaxLength(255);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.LastLoginAt);

            // Email and Username are already configured by IdentityUser
            builder.Property(u => u.Email)
                .HasMaxLength(256);

            builder.Property(u => u.UserName)
                .HasMaxLength(256);

            // Indexes
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.UserName)
                .IsUnique();

            builder.HasIndex(u => u.StripeCustomerId);

            builder.HasIndex(u => u.Role);

            // Relationships
            builder.HasOne(u => u.TrainerProfile)
                .WithOne(tp => tp.User)
                .HasForeignKey<TrainerProfile>(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.ClientProfile)
                .WithOne(cp => cp.User)
                .HasForeignKey<ClientProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Subscriptions)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(u => u.WorkoutLogs)
            //    .WithOne(wl => wl.Client)
            //    .HasForeignKey(wl => wl.ClientId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.BodyStatLogs)
            //    .WithOne(bsl => bsl.Client)
            //    .HasForeignKey(bsl => bsl.ClientId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

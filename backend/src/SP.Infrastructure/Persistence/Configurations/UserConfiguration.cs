// SP.Infrastructure/Persistence/Configurations/UserConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Users;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.BlockReason)
            .HasMaxLength(500);

        builder.Property(u => u.SuspendReason)
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.IsEmailVerified).IsRequired();
        builder.Property(u => u.IsDeleted).IsRequired();
        builder.Property(u => u.IsBlocked).IsRequired();
        builder.Property(u => u.IsSuspended).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        // Profile picture — owned value object
        builder.OwnsOne(u => u.UserProfilePicture, img =>
        {
            img.Property(i => i.Url)
                .HasColumnName("ProfilePictureUrl")
                .HasMaxLength(1000)
                .IsRequired()
                .HasDefaultValue("https://ui-avatars.com/api/?background=random");

            img.Property(i => i.IsMain)
                .HasColumnName("ProfilePictureIsMain")
                .IsRequired()
                .HasDefaultValue(true);
        });

        // Credential — owned entity (1-to-1, same table boundary via FK)
        builder.OwnsOne(u => u.Credential, cred =>
        {
            cred.ToTable("UserCredentials");
            cred.WithOwner().HasForeignKey("UserId");
            cred.HasKey("Id");

            cred.Property<Guid>("Id").ValueGeneratedNever();
            cred.Property(c => c.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            cred.Property(c => c.CreatedAt).IsRequired();
            cred.HasIndex("UserId").IsUnique();
        });

        // Refresh tokens — owned collection
        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("RefreshTokens");
            rt.WithOwner().HasForeignKey("UserId");
            rt.HasKey("Id");

            rt.Property<Guid>("Id").ValueGeneratedNever();
            rt.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(512);

            rt.Property(t => t.ExpiresAt).IsRequired();
            rt.Property(t => t.CreatedAt).IsRequired();

            rt.HasIndex(t => t.Token).IsUnique();
            rt.HasIndex("UserId");
        });

        // Soft-delete global query filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Latitude);
        builder.Property(u => u.Longitude);
        builder.Property(u => u.LastLocationUpdate);
    }
}

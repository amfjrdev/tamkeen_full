using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.ProviderProfiles;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ProviderProfileConfiguration : IEntityTypeConfiguration<ProviderProfile>
{
    public void Configure(EntityTypeBuilder<ProviderProfile> builder)
    {
        builder.ToTable("ProviderProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProviderId).IsRequired();

        builder.Property(p => p.AboutMe).HasMaxLength(1000);

        builder.Property(p => p.HourlyRate)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.IsAvailable).IsRequired();
        builder.Property(p => p.ResponseTimeMins).IsRequired();
        builder.Property(p => p.Latitude);
        builder.Property(p => p.Longitude);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => p.ProviderId).IsUnique();
        builder.HasIndex(p => p.IsAvailable);
    }
}

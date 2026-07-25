using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Connects;
using System.Collections.Generic;
using System.Text.Json;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ConnectPackConfiguration : IEntityTypeConfiguration<ConnectPack>
{
    public void Configure(EntityTypeBuilder<ConnectPack> builder)
    {
        builder.ToTable("ConnectPacks");
        builder.HasKey(p => p.Id);

        // Configure IntId as an identity column (auto-increment)
        builder.Property(p => p.IntId)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Credits)
            .IsRequired();

        builder.Property(p => p.Color)
            .HasMaxLength(20);

        builder.Property(p => p.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Map Features list of strings to JSON string in database with ValueComparer
        var valueComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null ? System.Linq.Enumerable.SequenceEqual(c1, c2) : c1 == c2,
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(p => p.Features)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
            )
            .Metadata.SetValueComparer(valueComparer);
    }
}

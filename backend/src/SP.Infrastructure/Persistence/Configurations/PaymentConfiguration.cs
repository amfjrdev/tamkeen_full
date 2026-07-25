using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Payments;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId).IsRequired();

        builder.Property(p => p.CheckoutId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PackId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(p => p.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CompletedAt);

        builder.Property(p => p.Version)
            .IsConcurrencyToken();

        builder.HasIndex(p => p.CheckoutId).IsUnique();
        builder.HasIndex(p => p.UserId);
    }
}

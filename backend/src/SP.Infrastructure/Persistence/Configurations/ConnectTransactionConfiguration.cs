using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Connects;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ConnectTransactionConfiguration : IEntityTypeConfiguration<ConnectTransaction>
{
    public void Configure(EntityTypeBuilder<ConnectTransaction> builder)
    {
        builder.ToTable("ConnectTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Amount).IsRequired();
        builder.Property(t => t.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.IdempotencyKey)
            .HasMaxLength(255);

        builder.Property(t => t.ReferenceId);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.UserId, t.CreatedAt });
    }
}

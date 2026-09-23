using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.ServiceRequests;
using SP.Domain.Users;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ServiceRequestApplicationConfiguration : IEntityTypeConfiguration<ServiceRequestApplication>
{
    public void Configure(EntityTypeBuilder<ServiceRequestApplication> builder)
    {
        builder.ToTable("ServiceRequestApplications");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CoverLetter)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.ProposedPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.ConnectsSpent)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique index: a provider can only apply once to a given service request
        builder.HasIndex(a => new { a.ServiceRequestId, a.ProviderId })
            .IsUnique();
    }
}

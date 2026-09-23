using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.ServiceRequests;
using SP.Domain.Users;
using SP.Domain.Categories;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("ServiceRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Wilaya)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Budget)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt).IsRequired();

        // Foreign keys
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.SelectedProviderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Navigation for owned applications
        builder.HasMany(r => r.Applications)
            .WithOne()
            .HasForeignKey(a => a.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation for review (1-to-1)
        builder.HasOne(r => r.Review)
            .WithOne()
            .HasForeignKey<ServiceRequestReview>(rev => rev.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for high performance querying
        builder.HasIndex(r => new { r.Wilaya, r.Status });
        builder.HasIndex(r => r.ClientId);
        builder.HasIndex(r => r.SelectedProviderId);
        builder.HasIndex(r => r.CreatedAt);
    }
}

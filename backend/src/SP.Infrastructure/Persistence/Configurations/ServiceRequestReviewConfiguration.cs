using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.ServiceRequests;
using SP.Domain.Users;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ServiceRequestReviewConfiguration : IEntityTypeConfiguration<ServiceRequestReview>
{
    public void Configure(EntityTypeBuilder<ServiceRequestReview> builder)
    {
        builder.ToTable("ServiceRequestReviews");
        builder.HasKey(rev => rev.Id);

        builder.Property(rev => rev.Rating).IsRequired();
        builder.Property(rev => rev.Comment).HasMaxLength(1000);
        builder.Property(rev => rev.CreatedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rev => rev.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rev => rev.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rev => rev.ServiceRequestId)
            .IsUnique();
    }
}

// SP.Infrastructure/Persistence/Configurations/PortfolioConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Portfolio;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolios");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProviderId).IsRequired();

        builder.HasIndex(p => p.ProviderId).IsUnique();

        // Owned Projects
        builder.OwnsMany(p => p.Projects, project =>
        {
            project.ToTable("Projects");
            project.WithOwner().HasForeignKey("PortfolioId");
            project.HasKey("Id");

            project.Property<Guid>("Id").ValueGeneratedNever();

            project.Property(pr => pr.Name)
                .IsRequired()
                .HasMaxLength(100);

            project.Property(pr => pr.Description)
                .IsRequired()
                .HasMaxLength(1000);

            project.HasIndex("PortfolioId");

            // Each Project owns its Images
            project.OwnsMany(pr => pr.ProjectImages, img =>
            {
                img.ToTable("ProjectImages");
                img.WithOwner().HasForeignKey("ProjectId");

                img.Property<Guid>("ImageId")
                    .ValueGeneratedOnAdd();
                img.HasKey("ImageId");

                img.Property(i => i.Url)
                    .IsRequired()
                    .HasMaxLength(1000);

                img.Property(i => i.IsMain).IsRequired();

                img.HasIndex("ProjectId");
            });
        });
    }
}

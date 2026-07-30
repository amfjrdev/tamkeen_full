// SP.Infrastructure/Persistence/Configurations/BookingConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Bookings;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.ClientId).IsRequired();
        builder.Property(b => b.ProviderId).IsRequired();
        builder.Property(b => b.ServiceId).IsRequired();
        builder.Property(b => b.ScheduledDate).IsRequired();
        builder.Property(b => b.RequestedAt).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Owned Review
        builder.OwnsOne(b => b.Review, review =>
        {
            review.ToTable("Reviews");
            review.WithOwner().HasForeignKey("BookingId");
            review.HasKey("Id");

            review.Property<Guid>("Id").ValueGeneratedNever();
            review.Property(r => r.ClientId).IsRequired();
            review.Property(r => r.ServiceId).IsRequired();
            review.Property(r => r.Rating).IsRequired();
            review.Property(r => r.Comment).HasMaxLength(1000);
            review.Property(r => r.CreatedAt).IsRequired();

            review.HasIndex("BookingId").IsUnique();
        });

        // Owned Report
        builder.OwnsOne(b => b.Report, report =>
        {
            report.ToTable("Reports");
            report.WithOwner().HasForeignKey("BookingId");
            report.HasKey("Id");

            report.Property<Guid>("Id").ValueGeneratedNever();
            report.Property(r => r.ReporterId).IsRequired();
            report.Property(r => r.Reason).IsRequired().HasMaxLength(1000);
            report.Property(r => r.CreatedAt).IsRequired();

            report.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            report.HasIndex("BookingId").IsUnique();
        });

        // Owned ChatRoom → owns ChatMessages
        builder.OwnsOne(b => b.ChatRoom, room =>
        {
            room.ToTable("ChatRooms");
            room.WithOwner().HasForeignKey("BookingId");
            room.HasKey("Id");

            room.Property<Guid>("Id").ValueGeneratedNever();
            room.Property(r => r.CreatedAt).IsRequired();

            room.HasIndex("BookingId").IsUnique();

            room.OwnsMany(r => r.Messages, msg =>
            {
                msg.ToTable("ChatMessages");
                msg.WithOwner().HasForeignKey("ChatRoomId");
                msg.HasKey("Id");

                msg.Property<Guid>("Id").ValueGeneratedNever();
                msg.Property(m => m.SenderId).IsRequired();
                msg.Property(m => m.Content).IsRequired().HasMaxLength(4000);
                msg.Property(m => m.SentAt).IsRequired();
                msg.Property(m => m.IsRead).IsRequired();

                msg.HasIndex("ChatRoomId");
            });
        });

        builder.HasIndex(b => b.ClientId);
        builder.HasIndex(b => b.ProviderId);
        builder.HasIndex(b => b.ServiceId);
        builder.HasIndex(b => b.Status);

        builder.HasOne<SP.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SP.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(b => b.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SP.Domain.Services.Service>()
            .WithMany()
            .HasForeignKey(b => b.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

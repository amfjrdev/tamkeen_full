// SP.Infrastructure/Persistence/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Bookings;
using SP.Domain.Categories;
using SP.Domain.Chat;
using SP.Domain.Notifications;
using SP.Domain.Portfolio;
using SP.Domain.ProviderProfiles;
using SP.Domain.Services;
using SP.Domain.ServiceRequests;
using SP.Domain.Shared;
using SP.Domain.Users;

namespace SP.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProviderProfile> ProviderProfiles => Set<ProviderProfile>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<SP.Domain.Chat.ChatMessage> ChatMessagesStandalone => Set<SP.Domain.Chat.ChatMessage>();
    public DbSet<SP.Domain.Connects.Wallet> Wallets => Set<SP.Domain.Connects.Wallet>();
    public DbSet<SP.Domain.Connects.ConnectTransaction> ConnectTransactions => Set<SP.Domain.Connects.ConnectTransaction>();
    public DbSet<SP.Domain.Payments.Payment> Payments => Set<SP.Domain.Payments.Payment>();
    public DbSet<AppConfiguration> AppConfigurations => Set<AppConfiguration>();
    public DbSet<SP.Domain.Connects.ConnectPack> ConnectPacks => Set<SP.Domain.Connects.ConnectPack>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceRequestApplication> ServiceRequestApplications => Set<ServiceRequestApplication>();
    public DbSet<ServiceRequestReview> ServiceRequestReviews => Set<ServiceRequestReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

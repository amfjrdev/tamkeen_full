using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SP.Domain.Bookings;
using SP.Domain.Categories;
using SP.Domain.Chat;
using SP.Domain.Connects;
using SP.Domain.Notifications;
using SP.Domain.Portfolio;
using SP.Domain.ProviderProfiles;
using SP.Domain.Services;
using SP.Domain.Shared;
using SP.Domain.Users;
using SP.Domain.Payments;
using SP.Infrastructure.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP.Infrastructure.Persistence;

public static class TestDataSeeder
{
    public static async Task SeedTestAccounts(ApplicationDbContext context, IConfiguration configuration)
    {
        // Unconditionally delete all extra client/provider accounts except defaults to clean the DB manually
        var defaultEmails = new[] { "admin@test.com", "client@test.com", "provider@test.com" };
        var extraUsers = await context.Users
            .Where(u => !defaultEmails.Contains(u.Email))
            .ToListAsync();

        if (extraUsers.Any())
        {
            var extraUserGuids = extraUsers.Select(u => u.Id).ToList();

            var relatedBookings = await context.Bookings.Where(b => extraUserGuids.Contains(b.ClientId) || extraUserGuids.Contains(b.ProviderId)).ToListAsync();
            context.Bookings.RemoveRange(relatedBookings);

            var relatedPayments = await context.Payments.Where(p => extraUserGuids.Contains(p.UserId)).ToListAsync();
            context.Payments.RemoveRange(relatedPayments);

            var relatedChats = await context.ChatMessagesStandalone.Where(m => extraUserGuids.Contains(m.SenderId)).ToListAsync();
            context.ChatMessagesStandalone.RemoveRange(relatedChats);

            var relatedConversations = await context.Conversations.Where(c => extraUserGuids.Contains(c.Participant1Id) || extraUserGuids.Contains(c.Participant2Id)).ToListAsync();
            context.Conversations.RemoveRange(relatedConversations);

            var relatedNotifications = await context.Notifications.Where(n => extraUserGuids.Contains(n.UserId)).ToListAsync();
            context.Notifications.RemoveRange(relatedNotifications);

            var relatedPortfolios = await context.Portfolios.Where(p => extraUserGuids.Contains(p.ProviderId)).ToListAsync();
            context.Portfolios.RemoveRange(relatedPortfolios);

            var relatedProfiles = await context.ProviderProfiles.Where(p => extraUserGuids.Contains(p.ProviderId)).ToListAsync();
            context.ProviderProfiles.RemoveRange(relatedProfiles);

            var relatedWallets = await context.Wallets.Where(w => extraUserGuids.Contains(w.UserId)).ToListAsync();
            context.Wallets.RemoveRange(relatedWallets);

            context.Users.RemoveRange(extraUsers);
            await context.SaveChangesAsync();
        }

        // Seed configurations first
        await ConfigurationSeeder.SeedConfigurationsAsync(context);

        // Seed default packages if none exist
        if (!await context.ConnectPacks.AnyAsync())
        {
            var defaultPacks = new List<ConnectPack>
            {
                ConnectPack.Create("starter", "Starter Pack", "Perfect for new providers getting started", 500, 5, "blue", new List<string> { "5 Connect Credits", "Valid for 30 days", "Email support", "Basic analytics" }, false, 10),
                ConnectPack.Create("medium", "Medium Pack", "Most popular choice for active providers", 1200, 15, "indigo", new List<string> { "15 Connect Credits", "Valid for 60 days", "Priority support", "Advanced analytics", "Featured badge" }, true, 30),
                ConnectPack.Create("premium", "Premium Pack", "For established providers with high demand", 3500, 50, "pink", new List<string> { "50 Connect Credits", "Valid for 90 days", "24/7 Priority support", "Premium analytics", "Featured badge", "Profile boost" }, false, 50)
            };
            context.ConnectPacks.AddRange(defaultPacks);
            await context.SaveChangesAsync();
        }

        // If bookings already exist, we assume the DB is already fully seeded
        if (await context.Bookings.AnyAsync())
            return;

        // Clear existing tables to prevent duplicate key violations and ensure clean relations
        context.ChatMessagesStandalone.RemoveRange(context.ChatMessagesStandalone);
        context.Conversations.RemoveRange(context.Conversations);
        context.Notifications.RemoveRange(context.Notifications);
        context.Payments.RemoveRange(context.Payments);
        context.Bookings.RemoveRange(context.Bookings);
        context.Portfolios.RemoveRange(context.Portfolios);
        context.ProviderProfiles.RemoveRange(context.ProviderProfiles);
        context.ConnectTransactions.RemoveRange(context.ConnectTransactions);
        context.Wallets.RemoveRange(context.Wallets);
        context.Services.RemoveRange(context.Services);
        context.Users.RemoveRange(context.Users);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        var seederSection = configuration.GetSection("Seeder");
        var defaultAvatarUrl = configuration["App:DefaultAvatarUrl"]
            ?? "https://ui-avatars.com/api/?background=random";

        var passwordHasher = new PasswordHasher();
        var defaultPicture = new Image(defaultAvatarUrl, true);

        // 1. Seed Categories
        var plumbingCat = Category.Create("Plumbing", "Water pipes, leaks, installations and repairs");
        var electricalCat = Category.Create("Electrical", "Wiring, installations, and electrical repairs");
        var cleaningCat = Category.Create("Cleaning", "Home and office deep cleaning services");
        var paintingCat = Category.Create("Painting", "Interior and exterior painting services");
        var carpentryCat = Category.Create("Carpentry", "Furniture, woodwork and custom carpentry");

        context.Categories.AddRange(plumbingCat, electricalCat, cleaningCat, paintingCat, carpentryCat);
        await context.SaveChangesAsync();

        // Helper to register users
        var seedUsers = new List<User>();
        User createUser(string email, string firstName, string lastName, string phoneNumber, UserRole role, string password)
        {
            var user = User.Create(email, firstName, lastName, phoneNumber, role).Value;
            user.VerifyEmail();
            user.UpdateProfilePicture(new Image($"https://api.dicebear.com/7.x/avataaars/png?seed={firstName}", true));
            user.SetCredential(passwordHasher.Hash(password));
            seedUsers.Add(user);
            return user;
        }

        // Admin
        var adminEmail = seederSection["AdminEmail"] ?? "admin@test.com";
        var adminPassword = seederSection["AdminPassword"] ?? "Admin123!";
        var admin = createUser(adminEmail, "System", "Administrator", "+1122334455", UserRole.Admin, adminPassword);

        // Client
        var clientEmail = seederSection["ClientEmail"] ?? "client@test.com";
        var clientPassword = seederSection["ClientPassword"] ?? "Client123!";
        var defaultClient = createUser(clientEmail, "Jane", "Client", "+0987654321", UserRole.Client, clientPassword);

        // Provider
        var providerEmail = seederSection["ProviderEmail"] ?? "provider@test.com";
        var providerPassword = seederSection["ProviderPassword"] ?? "Provider123!";
        var defaultProvider = createUser(providerEmail, "John", "Provider", "+1234567890", UserRole.Provider, providerPassword);

        context.Users.AddRange(seedUsers);
        await context.SaveChangesAsync();

        // 2. Seed ProviderProfiles, Portfolios, and Wallets
        var providersList = new[]
        {
            (User: defaultProvider, HourlyRate: 1200m, ResponseMins: 15, Lat: 36.7372, Lng: 3.0865, About: "Prestataire généraliste expérimenté disponible pour diverses interventions rapides à Alger.", ProjectTitle: "Réalisations Générales", ProjectDesc: "Aperçu de mes services réguliers.", ProjectImage: "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=500&auto=format&fit=crop&q=80")
        };

        foreach (var p in providersList)
        {
            // Wallet
            var wallet = Wallet.Create(p.User.Id, 100);
            context.Wallets.Add(wallet);

            // Connect Transaction
            var tx = ConnectTransaction.Create(p.User.Id, 100, "Credit", Guid.NewGuid().ToString(), null);
            context.ConnectTransactions.Add(tx);

            // Provider Profile
            var profile = ProviderProfile.Create(p.User.Id);
            profile.UpdateDetails(p.About, p.HourlyRate, p.ResponseMins);
            profile.SetAvailability(true);
            profile.SetLocation(p.Lat, p.Lng);
            context.ProviderProfiles.Add(profile);

            // Portfolio
            var portfolio = Portfolio.Create(p.User.Id);
            var images = new List<Image> { new Image(p.ProjectImage, true) };
            portfolio.AddProject(p.ProjectTitle, p.ProjectDesc, images);
            context.Portfolios.Add(portfolio);
        }
        await context.SaveChangesAsync();

        // 3. Seed Services
        var services = new List<Service>
        {
            // Default Provider Services
            Service.Create(defaultProvider.Id, plumbingCat.Id, "Réparation Fuite d'Eau", "Détection et réparation rapide de fuites sur tuyauteries cuivre, PVC et PER.", 1800, 60).Value,
            Service.Create(defaultProvider.Id, electricalCat.Id, "Dépannage Court-Circuit", "Recherche de panne électrique et remplacement des composants défectueux.", 2500, 60).Value,
            Service.Create(defaultProvider.Id, cleaningCat.Id, "Nettoyage Standard de Maison", "Aspiration, lavage des sols, dépoussiérage et nettoyage des sanitaires.", 4000, 180).Value
        };

        context.Services.AddRange(services);
        await context.SaveChangesAsync();

        // 4. Seed Standalone Conversations and Messages
        void seedChat(Guid clientUserId, Guid providerUserId, bool isLocked, List<string> messagesList)
        {
            var conv = Conversation.Create(clientUserId, providerUserId, 5);
            if (!isLocked) conv.Unlock();
            context.Conversations.Add(conv);

            SP.Domain.Chat.ChatMessage lastMsg = null;
            for (int i = 0; i < messagesList.Count; i++)
            {
                var senderId = (i % 2 == 0) ? clientUserId : providerUserId;
                var msg = SP.Domain.Chat.ChatMessage.Create(conv.Id, senderId, messagesList[i]);
                context.ChatMessagesStandalone.Add(msg);
                lastMsg = msg;
            }

            if (lastMsg != null)
            {
                conv.UpdateLastMessage(lastMsg.Text, lastMsg.SenderId, lastMsg.SentAt);
            }
        }

        seedChat(defaultClient.Id, defaultProvider.Id, false, new List<string>
        {
            "Bonjour, j'ai une fuite sous mon évier de cuisine. Êtes-vous disponible demain matin ?",
            "Bonjour Jane! Oui, tout à fait. Je peux passer vers 9h. C'est un tuyau en PVC ou en cuivre ?",
            "C'est un raccord en cuivre qui goutte. Je vous envoie une photo dès que possible.",
            "Très bien, préparez le terrain et je m'occupe du reste. À demain !"
        });

        await context.SaveChangesAsync();

        // 5. Seed Bookings, Payments, and Reviews
        void seedBookingCycle(Guid clientUserId, User providerUser, Service service, BookingStatus status, int? reviewRating, string? reviewComment)
        {
            var booking = Booking.Create(clientUserId, providerUser.Id, service.Id, DateTime.UtcNow.AddDays(3)).Value;
            context.Bookings.Add(booking);

            if (status == BookingStatus.Pending)
            {
                // Just keep it pending
            }
            else if (status == BookingStatus.Rejected)
            {
                booking.Reject();
            }
            else if (status == BookingStatus.Cancelled)
            {
                booking.Cancel();
            }
            else if (status == BookingStatus.Accepted || status == BookingStatus.Completed)
            {
                booking.Accept();

                // Send some messages
                booking.SendMessage(clientUserId, "Bonjour, je confirme pour notre rendez-vous.");
                booking.SendMessage(providerUser.Id, "Bonjour, c'est bien noté. Je serai là à l'heure.");

                if (status == BookingStatus.Completed)
                {
                    booking.Complete();

                    // Seed Payment
                    var payment = Payment.Create(clientUserId, $"ch_{Guid.NewGuid().ToString().Substring(0, 10)}", "https://pay.chargily.net/test/checkout", "pack_default", service.Price, "DZD", "Chargily");
                    payment.MarkAsPaid();
                    context.Payments.Add(payment);

                    // Seed Review if rating is provided
                    if (reviewRating.HasValue)
                    {
                        booking.AddReview(clientUserId, reviewRating.Value, reviewComment ?? string.Empty);
                    }
                }
            }
        }

        // Seed Bookings
        // Pending booking for defaultClient and defaultProvider
        seedBookingCycle(defaultClient.Id, defaultProvider, services[0], BookingStatus.Pending, null, null);

        // Accepted booking for defaultClient and defaultProvider (Active Chat)
        seedBookingCycle(defaultClient.Id, defaultProvider, services[1], BookingStatus.Accepted, null, null);

        // Completed booking for defaultClient and defaultProvider (shows review star)
        seedBookingCycle(defaultClient.Id, defaultProvider, services[2], BookingStatus.Completed, 5, "Magnifique ! Peinture impeccable, finitions soignées, nettoyage irréprochable après chantier.");

        // Seed Notification
        var notif1 = Notification.Create(defaultClient.Id, "Bienvenue sur Tamkeen", "Votre compte a été créé avec succès. Vous pouvez maintenant rechercher des prestataires.").Value;
        var notif2 = Notification.Create(defaultClient.Id, "Demande Acceptée", "John Provider a accepté votre demande de service.").Value;
        context.Notifications.AddRange(notif1, notif2);

        await context.SaveChangesAsync();
    }
}

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

        // Realistic Extra Clients
        var client1 = createUser("karim.hassan@test.com", "Karim", "Hassan", "+213550123456", UserRole.Client, "Client123!");
        var client2 = createUser("amina.kacimi@test.com", "Amina", "Kacimi", "+213661987654", UserRole.Client, "Client123!");
        var client3 = createUser("ryad.bensmail@test.com", "Ryad", "Bensmail", "+213770456123", UserRole.Client, "Client123!");
        var client4 = createUser("selma.khaldi@test.com", "Selma", "Khaldi", "+213555789123", UserRole.Client, "Client123!");

        // Realistic Extra Providers
        var pPlumber = createUser("ahmed.plumber@test.com", "Ahmed", "Benali", "+213551111111", UserRole.Provider, "Provider123!");
        var pElectrician = createUser("sara.electrician@test.com", "Sara", "Mansouri", "+213662222222", UserRole.Provider, "Provider123!");
        var pCleaner = createUser("karim.cleaner@test.com", "Karim", "Daoud", "+213773333333", UserRole.Provider, "Provider123!");
        var pPainter = createUser("lina.painter@test.com", "Lina", "Belkacem", "+213554444444", UserRole.Provider, "Provider123!");
        var pCarpenter = createUser("youcef.carpenter@test.com", "Youcef", "Slimani", "+213665555555", UserRole.Provider, "Provider123!");

        context.Users.AddRange(seedUsers);
        await context.SaveChangesAsync();

        // 2. Seed ProviderProfiles, Portfolios, and Wallets
        var providersList = new[]
        {
            (User: defaultProvider, HourlyRate: 1200m, ResponseMins: 15, Lat: 36.7372, Lng: 3.0865, About: "Prestataire généraliste expérimenté disponible pour diverses interventions rapides à Alger.", ProjectTitle: "Réalisations Générales", ProjectDesc: "Aperçu de mes services réguliers.", ProjectImage: "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=500&auto=format&fit=crop&q=80"),
            (User: pPlumber, HourlyRate: 1500m, ResponseMins: 10, Lat: 36.7525, Lng: 3.0420, About: "Artisan plombier chauffagiste certifié. Spécialiste de la recherche de fuites, dépannage en urgence et rénovation complète de salles de bain. Travail soigné et garanti.", ProjectTitle: "Rénovation de Salle de Bain", ProjectDesc: "Remplacement complet de la tuyauterie en cuivre et pose de douche italienne moderne.", ProjectImage: "https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=500&auto=format&fit=crop&q=80"),
            (User: pElectrician, HourlyRate: 1800m, ResponseMins: 20, Lat: 36.7201, Lng: 3.1012, About: "Électricienne professionnelle. Installation, mise en conformité, dépannage rapide et pose de luminaires intelligents. 8 ans d'expérience.", ProjectTitle: "Mise aux normes électriques", ProjectDesc: "Installation d'un nouveau tableau électrique avec disjoncteurs différentiels.", ProjectImage: "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=500&auto=format&fit=crop&q=80"),
            (User: pCleaner, HourlyRate: 1000m, ResponseMins: 30, Lat: 36.7410, Lng: 3.0750, About: "Services de nettoyage résidentiel et commercial. Nettoyage de fin de chantier, lavage de vitres et désinfection. Équipe sérieuse et dynamique.", ProjectTitle: "Nettoyage Fin de Chantier", ProjectDesc: "Remise en état complète d'un appartement de 120m² après travaux de rénovation.", ProjectImage: "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=500&auto=format&fit=crop&q=80"),
            (User: pPainter, HourlyRate: 1300m, ResponseMins: 25, Lat: 36.7610, Lng: 3.0210, About: "Artiste peintre en bâtiment. Spécialiste des peintures décoratives modernes (Sable, Stucco) et ravalement de façades. Devis gratuit.", ProjectTitle: "Peinture Décorative Salon", ProjectDesc: "Application d'une peinture sablée beige avec finitions soignées.", ProjectImage: "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=500&auto=format&fit=crop&q=80"),
            (User: pCarpenter, HourlyRate: 2000m, ResponseMins: 40, Lat: 36.7115, Lng: 3.0915, About: "Ébéniste menuisier passionné. Fabrication de meubles sur mesure, montage de cuisines équipées et réparation de menuiseries en bois.", ProjectTitle: "Conception Dressing", ProjectDesc: "Dressing sur mesure en chêne massif avec éclairage LED intégré.", ProjectImage: "https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=500&auto=format&fit=crop&q=80")
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
            // Plumber Services
            Service.Create(pPlumber.Id, plumbingCat.Id, "Réparation Fuite d'Eau", "Détection et réparation rapide de fuites sur tuyauteries cuivre, PVC et PER.", 1800, 60).Value,
            Service.Create(pPlumber.Id, plumbingCat.Id, "Installation de Robinet", "Remplacement et pose de mitigeurs, mélangeurs et robinets d'arrêt.", 2500, 45).Value,
            Service.Create(pPlumber.Id, plumbingCat.Id, "Débouchage Canalisation", "Désengorgement de lavabos, éviers, baignoires et WC avec furet professionnel.", 3000, 90).Value,

            // Electrician Services
            Service.Create(pElectrician.Id, electricalCat.Id, "Dépannage Court-Circuit", "Recherche de panne électrique et remplacement des composants défectueux.", 2500, 60).Value,
            Service.Create(pElectrician.Id, electricalCat.Id, "Remplacement de Tableau Électrique", "Mise en conformité complète du tableau avec disjoncteurs différentiels modernes.", 12000, 180).Value,
            Service.Create(pElectrician.Id, electricalCat.Id, "Pose de Luminaire", "Installation sécurisée de lustres, spots LED et appliques murales.", 1500, 30).Value,

            // Cleaner Services
            Service.Create(pCleaner.Id, cleaningCat.Id, "Nettoyage Standard de Maison", "Aspiration, lavage des sols, dépoussiérage et nettoyage des sanitaires.", 4000, 180).Value,
            Service.Create(pCleaner.Id, cleaningCat.Id, "Nettoyage Profond / Après Fêtes", "Grand nettoyage de printemps incluant vitres, four et placards en profondeur.", 8000, 300).Value,

            // Painter Services
            Service.Create(pPainter.Id, paintingCat.Id, "Peinture de Chambre (Murs)", "Préparation des supports (enduit, ponçage) et application de deux couches de peinture acrylique.", 9500, 360).Value,
            Service.Create(pPainter.Id, paintingCat.Id, "Rénovation Peinture Salon", "Peinture complète murs et plafond avec finitions satinées ou mates de haute qualité.", 18000, 720).Value,

            // Carpenter Services
            Service.Create(pCarpenter.Id, carpentryCat.Id, "Montage de Meuble en Kit", "Assemblage rapide et solide de tous vos meubles (IKEA, etc.).", 2500, 90).Value,
            Service.Create(pCarpenter.Id, carpentryCat.Id, "Rabotage de Portes", "Ajustement et rabotage des portes en bois qui frottent sur le sol.", 2000, 45).Value,

            // Default Provider Services
            Service.Create(defaultProvider.Id, plumbingCat.Id, "Réparation Fuite Standard", "Service de plomberie express.", 1500, 60).Value,
            Service.Create(defaultProvider.Id, electricalCat.Id, "Diagnostic Électrique", "Recherche de pannes courantes.", 2000, 60).Value
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

        seedChat(defaultClient.Id, pPlumber.Id, false, new List<string>
        {
            "Bonjour, j'ai une fuite sous mon évier de cuisine. Êtes-vous disponible demain matin ?",
            "Bonjour Jane! Oui, tout à fait. Je peux passer vers 9h. C'est un tuyau en PVC ou en cuivre ?",
            "C'est un raccord en cuivre qui goutte. Je vous envoie une photo dès que possible.",
            "Très bien, préparez le terrain et je m'occupe du reste. À demain !"
        });

        seedChat(defaultClient.Id, pElectrician.Id, true, new List<string>
        {
            "Bonjour, j'aimerais changer mon tableau électrique complet.",
            "Bonjour, c'est possible. Quel est le nombre de disjoncteurs actuel ?"
        });

        seedChat(client1.Id, pCleaner.Id, false, new List<string>
        {
            "Bonjour, j'ai besoin d'un nettoyage complet de mon appartement ce samedi.",
            "Bonjour Karim, aucun problème. C'est quelle surface ?"
        });

        await context.SaveChangesAsync();

        // 5. Seed Bookings, Payments, and Reviews
        // Helper to seed booking with complete cycle
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
        // Pending booking for defaultClient and Ahmed Plumber
        seedBookingCycle(defaultClient.Id, pPlumber, services[0], BookingStatus.Pending, null, null);

        // Accepted booking for defaultClient and Ahmed Plumber (Active Chat)
        seedBookingCycle(defaultClient.Id, pPlumber, services[1], BookingStatus.Accepted, null, null);

        // Completed booking with 5-star review
        seedBookingCycle(defaultClient.Id, pPlumber, services[2], BookingStatus.Completed, 5, "Excellent travail! Ahmed a débouché les canalisations rapidement avec beaucoup de professionnalisme. Je recommande vivement.");

        // Completed booking for client1 and Sara Electrician (4-star review)
        seedBookingCycle(client1.Id, pElectrician, services[3], BookingStatus.Completed, 4, "Bonne prestation générale. Le court-circuit a été trouvé et réparé. Un peu en retard sur l'heure convenue mais très sympathique.");

        // Completed booking for client2 and Lina Painter (5-star review)
        seedBookingCycle(client2.Id, pPainter, services[8], BookingStatus.Completed, 5, "Magnifique ! Peinture impeccable, finitions soignées, nettoyage irréprochable après chantier.");

        // Cancelled booking
        seedBookingCycle(defaultClient.Id, pCarpenter, services[10], BookingStatus.Cancelled, null, null);

        // Rejected booking
        seedBookingCycle(defaultClient.Id, pElectrician, services[5], BookingStatus.Rejected, null, null);

        // Seed Notification
        var notif1 = Notification.Create(defaultClient.Id, "Bienvenue sur Tamkeen", "Votre compte a été créé avec succès. Vous pouvez maintenant rechercher des prestataires.").Value;
        var notif2 = Notification.Create(defaultClient.Id, "Demande Acceptée", "Ahmed Benali a accepté votre demande de service pour le raccord de robinet.").Value;
        context.Notifications.AddRange(notif1, notif2);

        await context.SaveChangesAsync();
    }
}

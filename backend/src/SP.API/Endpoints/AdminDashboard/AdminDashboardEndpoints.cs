using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP.Infrastructure.Persistence;
using SP.Domain.Users;
using SP.Domain.Categories;
using SP.Domain.Payments;
using SP.Domain.Payments.Enums;
using SP.Domain.Bookings;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace SP.API.Endpoints.AdminDashboard;

public static class AdminDashboardEndpoints
{
    public static RouteGroupBuilder MapAdminDashboardEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/dashboard", GetDashboardData).AllowAnonymous();
        group.MapGet("/messaging", GetMessagingData).AllowAnonymous();
        group.MapGet("/revenue", GetRevenueData).AllowAnonymous();
        group.MapGet("/transactions", GetTransactionsData).AllowAnonymous();
        group.MapGet("/packages", GetPackagesData).AllowAnonymous();
        group.MapPost("/packages", CreatePackage).AllowAnonymous();
        group.MapPut("/packages/{id:int}", UpdatePackage).AllowAnonymous();
        group.MapDelete("/packages/{id:int}", DeletePackage).AllowAnonymous();
        group.MapPost("/packages/{id:int}/toggle-status", TogglePackageStatus).AllowAnonymous();
        group.MapGet("/analytics", GetAnalyticsData).AllowAnonymous();

        // New dashboard endpoints
        group.MapGet("/admin-dashboard/users", GetUsersData).AllowAnonymous();
        group.MapGet("/admin-dashboard/clients", GetUsersData).AllowAnonymous();
        group.MapGet("/admin-dashboard/providers", GetProvidersData).AllowAnonymous();
        group.MapGet("/admin-dashboard/categories", GetCategoriesData).AllowAnonymous();
        group.MapPost("/admin-dashboard/categories", CreateCategory).AllowAnonymous();
        group.MapDelete("/admin-dashboard/categories/{id}", DeleteCategory).AllowAnonymous();
        group.MapPost("/admin-dashboard/conversations/{id:guid}/toggle-lock", ToggleConversationLock).AllowAnonymous();
        group.MapPost("/admin-dashboard/reports/{id:guid}/resolve", ResolveReport).AllowAnonymous();

        return group;
    }

    private static string GetTimeAgo(DateTime dt, DateTime now)
    {
        var span = now - dt;
        if (span.TotalDays >= 1) return $"{(int)span.TotalDays} days ago";
        if (span.TotalHours >= 1) return $"{(int)span.TotalHours} hours ago";
        if (span.TotalMinutes >= 1) return $"{(int)span.TotalMinutes} min ago";
        return "Just now";
    }

    private static string GetPercentageChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? "+100.0%" : "+0.0%";
        var diff = ((current - previous) / previous) * 100;
        return $"{(diff >= 0 ? "+" : "")}{diff:F1}%";
    }

    private static async Task<IResult> GetDashboardData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var totalUsers = await dbContext.Users.CountAsync(ct);
        var activeProviders = await dbContext.Users.CountAsync(u => u.Role == UserRole.Provider, ct);
        var totalRevenueAmount = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.Amount, ct);

        var totalRevenueStr = totalRevenueAmount > 0 
            ? $"{totalRevenueAmount:N0} DA" 
            : "0 DA";
        
        var usersCountStr = totalUsers.ToString("N0");
        var providersCountStr = activeProviders.ToString("N0");

        // Trends calculations
        var revenueThisMonth = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfMonth)
            .SumAsync(p => p.Amount, ct);
        var revenueLastMonth = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfMonth)
            .SumAsync(p => p.Amount, ct);
        var revenueChangeTrend = GetPercentageChange(revenueThisMonth, revenueLastMonth);

        var usersThisMonth = await dbContext.Users.CountAsync(u => u.CreatedAt >= startOfMonth, ct);
        var usersBeforeThisMonth = await dbContext.Users.CountAsync(u => u.CreatedAt < startOfMonth, ct);
        var usersChangeTrend = GetPercentageChange(usersThisMonth, usersBeforeThisMonth);

        var providersThisMonth = await dbContext.Users.CountAsync(u => u.Role == UserRole.Provider && u.CreatedAt >= startOfMonth, ct);
        var providersBeforeThisMonth = await dbContext.Users.CountAsync(u => u.Role == UserRole.Provider && u.CreatedAt < startOfMonth, ct);
        var providersChangeTrend = GetPercentageChange(providersThisMonth, providersBeforeThisMonth);

        var startOfToday = now.Date;
        var startOfYesterday = startOfToday.AddDays(-1);
        var messagesToday = await dbContext.ChatMessagesStandalone.CountAsync(m => m.SentAt >= startOfToday, ct);
        var messagesYesterday = await dbContext.ChatMessagesStandalone.CountAsync(m => m.SentAt >= startOfYesterday && m.SentAt < startOfToday, ct);
        var messagesChangeTrend = GetPercentageChange(messagesToday, messagesYesterday);

        // Service category distribution
        var categoryCounts = await dbContext.Bookings
            .Join(dbContext.Services, b => b.ServiceId, s => s.Id, (b, s) => new { b, s })
            .Join(dbContext.Categories, x => x.s.CategoryId, c => c.Id, (x, c) => new { c.Name, CategoryId = c.Id })
            .GroupBy(x => new { x.CategoryId, x.Name })
            .Select(g => new
            {
                name = g.Key.Name,
                value = g.Count()
            })
            .OrderByDescending(x => x.value)
            .Take(5)
            .ToListAsync(ct);

        var colors = new[] { "#6366f1", "#8b5cf6", "#ec4899", "#f59e0b", "#34d399" };
        var serviceCategories = categoryCounts.Select((cat, idx) => new
        {
            name = cat.name,
            value = cat.value,
            color = colors[idx % colors.Length]
        }).ToList();

        // Recent Activity combining new users and bookings
        var recentUsers = await dbContext.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new
            {
                CreatedAt = u.CreatedAt,
                Name = $"{u.FirstName} {u.LastName}",
                Description = u.Role == UserRole.Provider ? "New provider registered" : "New client registered",
                Type = u.Role == UserRole.Provider ? "pending" : "success"
            })
            .ToListAsync(ct);

        var recentBookings = await dbContext.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new
            {
                CreatedAt = b.CreatedAt,
                Name = "Booking Request",
                Description = $"Booking {b.Status.ToString().ToLower()}",
                Type = b.Status == BookingStatus.Completed ? "success" : b.Status == BookingStatus.Pending ? "pending" : "info"
            })
            .ToListAsync(ct);

        var combinedActivity = recentUsers.Select(r => new { r.CreatedAt, r.Name, r.Description, r.Type })
            .Concat(recentBookings.Select(b => new { b.CreatedAt, Name = b.Name, Description = b.Description, Type = b.Type }))
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select((x, idx) => new
            {
                id = idx + 1,
                name = x.Name,
                description = x.Description,
                time = GetTimeAgo(x.CreatedAt, now),
                type = x.Type
            })
            .ToList();



        // Weekly activity logs (last 7 days)
        var dates = Enumerable.Range(0, 7).Select(i => now.Date.AddDays(-6 + i)).ToList();
        var weekStart = now.Date.AddDays(-6);
        
        var dailyUsers = await dbContext.Users
            .Where(u => u.CreatedAt >= weekStart)
            .Select(u => new { Date = u.CreatedAt.Date })
            .GroupBy(x => x.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, ct);

        var dailyRequests = await dbContext.Bookings
            .Where(b => b.CreatedAt >= weekStart)
            .Select(b => new { Date = b.CreatedAt.Date })
            .GroupBy(x => x.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, ct);

        var dailyMessages = await dbContext.ChatMessagesStandalone
            .Where(m => m.SentAt >= weekStart)
            .Select(m => new { Date = m.SentAt.Date })
            .GroupBy(x => x.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, ct);

        var weeklyLabels = dates.Select(d => d.ToString("ddd")).ToArray();
        var weeklyUsers = dates.Select(d => dailyUsers.TryGetValue(d, out var c) ? c : 0).ToArray();
        var weeklyRequests = dates.Select(d => dailyRequests.TryGetValue(d, out var c) ? c : 0).ToArray();
        var weeklyMessages = dates.Select(d => dailyMessages.TryGetValue(d, out var c) ? c : 0).ToArray();

        // Conversion rates for last 5 months
        var months = Enumerable.Range(0, 5).Select(i => now.AddMonths(-4 + i)).ToList();
        var conversionRate = new List<object>();
        foreach (var m in months)
        {
            var startOfM = new DateTime(m.Year, m.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfM = startOfM.AddMonths(1);
            var totalBookings = await dbContext.Bookings.CountAsync(b => b.CreatedAt >= startOfM && b.CreatedAt < endOfM, ct);
            var completedBookings = await dbContext.Bookings.CountAsync(b => b.Status == BookingStatus.Completed && b.CreatedAt >= startOfM && b.CreatedAt < endOfM, ct);

            var rate = totalBookings > 0 ? (int)Math.Round((double)completedBookings / totalBookings * 100) : 0;
            conversionRate.Add(new { month = m.ToString("MMM"), value = rate });
        }

        return Results.Ok(new
        {
            stats = new[]
            {
                new { label = "Total Revenue", value = totalRevenueStr, change = revenueChangeTrend, icon = "revenue", color = "green" },
                new { label = "Total Users", value = usersCountStr, change = usersChangeTrend, icon = "users", color = "blue" },
                new { label = "Active Providers", value = providersCountStr, change = providersChangeTrend, icon = "providers", color = "purple" },
                new { label = "Messages Today", value = messagesToday.ToString("N0"), change = messagesChangeTrend, icon = "messages", color = "pink" }
            },
            weeklyActivity = new
            {
                labels = weeklyLabels,
                users = weeklyUsers,
                requests = weeklyRequests,
                messages = weeklyMessages
            },
            serviceCategories,
            conversionRate,
            recentActivity = combinedActivity,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = true },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    private static async Task<IResult> GetMessagingData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfToday = now.Date;

        var totalConversations = await dbContext.Conversations.CountAsync(ct);
        var activeConversationsCountStr = totalConversations.ToString("N0");

        var dbConversations = await dbContext.Conversations
            .OrderByDescending(c => c.LastMessageSentAt ?? c.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        var participantIds = dbConversations.Select(c => c.Participant1Id)
            .Concat(dbConversations.Select(c => c.Participant2Id))
            .Distinct()
            .ToList();

        var usersMap = await dbContext.Users
            .Where(u => participantIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}", ct);

        var conversationIds = dbConversations.Select(c => c.Id).ToList();
        var messageCounts = await dbContext.ChatMessagesStandalone
            .Where(m => conversationIds.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count, ct);

        var conversations = dbConversations.Select((c, idx) => new
        {
            id = idx + 1,
            participant1 = usersMap.TryGetValue(c.Participant1Id, out var p1) ? p1 : "Seeded Client",
            participant2 = usersMap.TryGetValue(c.Participant2Id, out var p2) ? p2 : "Seeded Provider",
            lastMessage = c.LastMessageText ?? "No messages yet",
            time = c.LastMessageSentAt.HasValue ? GetTimeAgo(c.LastMessageSentAt.Value, now) : "Just now",
            messageCount = messageCounts.TryGetValue(c.Id, out var count) ? count : 0,
            status = c.IsLocked ? "reported" : "active",
            isLocked = c.IsLocked
        }).ToList();



        var messagesToday = await dbContext.ChatMessagesStandalone.CountAsync(m => m.SentAt >= startOfToday, ct);
        var reportedCount = await dbContext.Bookings.CountAsync(b => b.Report != null && b.Report.Status == ReportStatus.Open, ct);
        var lockedChatsCount = await dbContext.Conversations.CountAsync(c => c.IsLocked, ct);

        var openReports = await dbContext.Bookings
            .Include(b => b.Report)
            .Where(b => b.Report != null && b.Report.Status == ReportStatus.Open)
            .OrderByDescending(b => b.Report!.CreatedAt)
            .ToListAsync(ct);

        var reportClientIds = openReports.Select(b => b.ClientId).Distinct().ToList();
        var reportProviderIds = openReports.Select(b => b.ProviderId).Distinct().ToList();
        var reportUserIds = reportClientIds.Concat(reportProviderIds).Concat(openReports.Select(b => b.Report!.ReporterId)).Distinct().ToList();

        var reportUsersMap = await dbContext.Users
            .Where(u => reportUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}", ct);

        var reportedMessages = openReports.Select((b, idx) => new
        {
            id = b.Report!.Id.ToString(),
            participant1 = reportUsersMap.TryGetValue(b.ClientId, out var clientName) ? clientName : "Client",
            participant2 = reportUsersMap.TryGetValue(b.ProviderId, out var providerName) ? providerName : "Provider",
            time = GetTimeAgo(b.Report!.CreatedAt, now),
            content = $"Booking dispute report: {b.Report.Reason}",
            reportedBy = reportUsersMap.TryGetValue(b.Report.ReporterId, out var reporterName) ? reporterName : "User",
            reason = b.Report.Reason
        }).ToList();

        return Results.Ok(new
        {
            stats = new[]
            {
                new { id = 1, label = "Active Chats", value = activeConversationsCountStr, trend = "+0%", icon = "chat", color = "green" },
                new { id = 2, label = "Messages Today", value = messagesToday.ToString("N0"), trend = "+0%", icon = "message", color = "blue" },
                new { id = 3, label = "Reported", value = reportedCount.ToString(), trend = "0", icon = "alert", color = "red" },
                new { id = 4, label = "Locked Chats", value = lockedChatsCount.ToString(), trend = "0", icon = "lock", color = "orange" }
            },
            conversations,
            reportedMessages,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = true },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    private static async Task<IResult> GetRevenueData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfToday = now.Date;
        var startOfYesterday = startOfToday.AddDays(-1);
        var startOfWeek = now.Date.AddDays(-6);
        var startOfLastWeek = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastYear = startOfYear.AddYears(-1);

        // Calculate Revenue stats from Payments
        var totalRevenueAmount = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.Amount, ct);

        var totalRevStr = totalRevenueAmount > 0 
            ? $"{totalRevenueAmount:N0} DA" 
            : "0 DA";

        var revenueToday = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfToday).SumAsync(p => p.Amount, ct);
        var revenueYesterday = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfYesterday && p.CreatedAt < startOfToday).SumAsync(p => p.Amount, ct);
        var trendToday = GetPercentageChange(revenueToday, revenueYesterday);

        var revenueThisWeek = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfWeek).SumAsync(p => p.Amount, ct);
        var revenueLastWeek = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfLastWeek && p.CreatedAt < startOfWeek).SumAsync(p => p.Amount, ct);
        var trendWeek = GetPercentageChange(revenueThisWeek, revenueLastWeek);

        var revenueThisMonth = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfMonth).SumAsync(p => p.Amount, ct);
        var revenueLastMonth = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfMonth).SumAsync(p => p.Amount, ct);
        var trendMonth = GetPercentageChange(revenueThisMonth, revenueLastMonth);

        var revenueThisYear = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfYear).SumAsync(p => p.Amount, ct);
        var revenueLastYear = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfLastYear && p.CreatedAt < startOfYear).SumAsync(p => p.Amount, ct);
        var trendYear = GetPercentageChange(revenueThisYear, revenueLastYear);

        var creditsThisMonth = await dbContext.ConnectTransactions.Where(t => t.Type == "Purchase" && t.CreatedAt >= startOfMonth).SumAsync(t => t.Amount, ct);
        var creditsLastMonth = await dbContext.ConnectTransactions.Where(t => t.Type == "Purchase" && t.CreatedAt >= startOfLastMonth && t.CreatedAt < startOfMonth).SumAsync(t => t.Amount, ct);
        var trendCredits = GetPercentageChange(creditsThisMonth, creditsLastMonth);

        var connectCreditsSold = await dbContext.ConnectTransactions.Where(t => t.Type == "Purchase").SumAsync(t => t.Amount, ct);

        var providerCount = await dbContext.Users.CountAsync(u => u.Role == UserRole.Provider, ct);
        var avgRevenuePerProvider = providerCount > 0 ? (totalRevenueAmount / providerCount) : 0;
        var avgRevenueThisMonth = providerCount > 0 ? (revenueThisMonth / providerCount) : 0;
        var avgRevenueLastMonth = providerCount > 0 ? (revenueLastMonth / providerCount) : 0;
        var trendAvgRevenue = GetPercentageChange(avgRevenueThisMonth, avgRevenueLastMonth);

        var successfulPayments = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid, ct);
        var successfulThisMonth = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfMonth, ct);
        var successfulLastMonth = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfMonth, ct);
        var trendSuccessful = GetPercentageChange(successfulThisMonth, successfulLastMonth);

        var failedPayments = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Failed, ct);
        var failedThisMonth = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Failed && p.CreatedAt >= startOfMonth, ct);
        var failedLastMonth = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Failed && p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfMonth, ct);
        var trendFailed = GetPercentageChange(failedThisMonth, failedLastMonth);

        var pendingTransactions = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Pending, ct);
        var pendingThisMonth = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Pending && p.CreatedAt >= startOfMonth, ct);
        var pendingLastMonth = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Pending && p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfMonth, ct);
        var trendPending = GetPercentageChange(pendingThisMonth, pendingLastMonth);

        // Top earning/spending providers
        var providerPayments = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .GroupBy(p => p.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Spent = g.Sum(x => x.Amount),
                PackagesCount = g.Count(x => x.PackId != null)
            })
            .OrderByDescending(x => x.Spent)
            .Take(5)
            .ToListAsync(ct);

        var topProviderIds = providerPayments.Select(x => x.UserId).ToList();
        var topProvidersMap = await dbContext.Users
            .Where(u => topProviderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}", ct);

        var providerCreditsMap = await dbContext.Wallets
            .Where(w => topProviderIds.Contains(w.UserId))
            .ToDictionaryAsync(w => w.UserId, w => w.Balance, ct);

        var topProviders = providerPayments.Select((x, idx) => new
        {
            rank = idx + 1,
            name = topProvidersMap.TryGetValue(x.UserId, out var name) ? name : "Unknown Provider",
            spent = (int)x.Spent,
            credits = providerCreditsMap.TryGetValue(x.UserId, out var bal) ? bal : 0,
            packages = x.PackagesCount
        }).ToList();

        // Monthly growthData (5 months)
        var monthsList = Enumerable.Range(0, 5).Select(i => now.AddMonths(-4 + i)).ToList();
        var growthData = new List<object>();
        foreach (var m in monthsList)
        {
            var startOfM = new DateTime(m.Year, m.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfM = startOfM.AddMonths(1);
            var totalRev = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfM && p.CreatedAt < endOfM).SumAsync(p => p.Amount, ct);
            growthData.Add(new
            {
                month = m.ToString("MMM"),
                revenue = (int)totalRev,
                forecast = (int)(totalRev * 1.15m)
            });
        }

        // Daily revenue (last 7 days)
        var dailyRev = new List<object>();
        for (int i = 0; i < 7; i++)
        {
            var date = now.Date.AddDays(-6 + i);
            var nextDate = date.AddDays(1);
            var amt = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= date && p.CreatedAt < nextDate).SumAsync(p => p.Amount, ct);
            dailyRev.Add(new
            {
                day = date.ToString("ddd"),
                value = (int)amt
            });
        }

        // Sales trend package types counts monthly
        var salesTrend = new List<object>();
        foreach (var m in monthsList)
        {
            var startOfM = new DateTime(m.Year, m.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfM = startOfM.AddMonths(1);
            var starter = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid && p.PackId != null && p.PackId.ToLower().Contains("starter") && p.CreatedAt >= startOfM && p.CreatedAt < endOfM, ct);
            var professional = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid && p.PackId != null && (p.PackId.ToLower().Contains("professional") || p.PackId.ToLower().Contains("medium")) && p.CreatedAt >= startOfM && p.CreatedAt < endOfM, ct);
            var premium = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid && p.PackId != null && p.PackId.ToLower().Contains("premium") && p.CreatedAt >= startOfM && p.CreatedAt < endOfM, ct);
            var enterprise = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid && p.PackId != null && p.PackId.ToLower().Contains("enterprise") && p.CreatedAt >= startOfM && p.CreatedAt < endOfM, ct);

            salesTrend.Add(new
            {
                month = m.ToString("MMM"),
                starter,
                professional,
                premium,
                enterprise
            });
        }

        // Payment methods grouped by Provider
        var methodCounts = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .GroupBy(p => p.Provider)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var totalMethodsCount = methodCounts.Sum(x => x.Count);
        var paymentMethods = methodCounts.Select((x, idx) => new
        {
            name = x.Name,
            value = totalMethodsCount > 0 ? (int)Math.Round((double)x.Count / totalMethodsCount * 100) : 0,
            color = idx switch
            {
                0 => "#6366f1",
                1 => "#3b82f6",
                2 => "#ec4899",
                _ => "#f59e0b"
            }
        }).ToList();

        // Breakdown categories sum
        var breakdownStarter = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.PackId != null && p.PackId.ToLower().Contains("starter")).SumAsync(p => p.Amount, ct);
        var breakdownProfessional = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.PackId != null && (p.PackId.ToLower().Contains("professional") || p.PackId.ToLower().Contains("medium"))).SumAsync(p => p.Amount, ct);
        var breakdownPremium = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.PackId != null && p.PackId.ToLower().Contains("premium")).SumAsync(p => p.Amount, ct);
        var breakdownEnterprise = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.PackId != null && p.PackId.ToLower().Contains("enterprise")).SumAsync(p => p.Amount, ct);

        var breakdown = new
        {
            starter = (int)breakdownStarter,
            professional = (int)breakdownProfessional,
            premium = (int)breakdownPremium,
            enterprise = (int)breakdownEnterprise
        };

        // Earnings
        var earningsFees = -(int)(totalRevenueAmount * 0.03m);
        var earningsNet = (int)totalRevenueAmount + earningsFees;
        var earnings = new
        {
            gross = (int)totalRevenueAmount,
            fees = earningsFees,
            refunds = 0,
            net = earningsNet
        };

        // Profit
        var profitOperating = (int)(totalRevenueAmount * 0.10m);
        var profitMarketing = (int)(totalRevenueAmount * 0.05m);
        var profitNet = earningsNet - profitOperating - profitMarketing;
        var profit = new
        {
            operating = profitOperating,
            marketing = profitMarketing,
            other = 0,
            net = profitNet
        };

        return Results.Ok(new
        {
            stats = new[]
            {
                new { id = 1, label = "Total Revenue", value = totalRevStr, trend = trendMonth, icon = "dollar", color = "green", negative = false },
                new { id = 2, label = "Revenue Today", value = $"{(int)revenueToday:N0} DA", trend = trendToday, icon = "chart-up", color = "blue", negative = false },
                new { id = 3, label = "Revenue This Week", value = $"{(int)revenueThisWeek:N0} DA", trend = trendWeek, icon = "chart-up", color = "purple", negative = false },
                new { id = 4, label = "Revenue This Month", value = $"{(int)revenueThisMonth:N0} DA", trend = trendMonth, icon = "chart-up", color = "pink", negative = false },
                new { id = 5, label = "Revenue This Year", value = $"{(int)revenueThisYear:N0} DA", trend = trendYear, icon = "chart-up", color = "orange", negative = false },
                new { id = 6, label = "Connect Credits Sold", value = ((int)connectCreditsSold).ToString("N0"), trend = trendCredits, icon = "credit-card", color = "pink", negative = false },
                new { id = 7, label = "Avg Revenue/Provider", value = $"{(int)avgRevenuePerProvider:N0} DA", trend = trendAvgRevenue, icon = "dollar", color = "blue", negative = false },
                new { id = 8, label = "Successful Payments", value = successfulPayments.ToString("N0"), trend = trendSuccessful, icon = "check-circle", color = "green", negative = false },
                new { id = 9, label = "Failed Payments", value = failedPayments.ToString("N0"), trend = trendFailed, icon = "alert-circle", color = "orange", negative = true },
                new { id = 10, label = "Pending Transactions", value = pendingTransactions.ToString("N0"), trend = trendPending, icon = "clock", color = "yellow", negative = false }
            },
            growthData,
            dailyRevenue = dailyRev,
            paymentMethods,
            salesTrend,
            topProviders,
            breakdown,
            earnings,
            profit,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = true },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false }
            }
        });
    }

    private static async Task<IResult> GetTransactionsData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var dbPayments = await dbContext.Payments
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        var userIds = dbPayments.Select(p => p.UserId).Distinct().ToList();
        var usersMap = await dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}", ct);

        var packsMap = await dbContext.ConnectPacks
            .ToDictionaryAsync(cp => cp.Code.ToLowerInvariant(), cp => cp.Credits, ct);

        var connectsMapFallback = new Dictionary<string, int>
        {
            { "starter", 5 },
            { "medium", 15 },
            { "premium", 50 }
        };

        var getCredits = (string? packId) =>
        {
            if (string.IsNullOrWhiteSpace(packId)) return 0;
            var normalized = packId.ToLowerInvariant().Trim();
            if (packsMap.TryGetValue(normalized, out var dbCredits)) return dbCredits;
            if (connectsMapFallback.TryGetValue(normalized, out var mapCredits)) return mapCredits;
            return 0;
        };

        var transactions = dbPayments.Select(p => new
        {
            id = p.CheckoutId,
            provider = usersMap.TryGetValue(p.UserId, out var name) ? name : "Seeded Provider",
            @package = p.PackId ?? "Starter Package",
            credits = getCredits(p.PackId),
            amount = (int)p.Amount,
            method = p.Provider == "Chargily" ? "Credit Card" : "PayPal",
            status = p.Status.ToString().ToLower(),
            date = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();



        var activity = dbPayments.Take(5).Select((p, idx) => new
        {
            id = idx + 1,
            user = usersMap.TryGetValue(p.UserId, out var name) ? name : "Seeded Provider",
            @package = p.PackId ?? "Starter Package",
            credits = getCredits(p.PackId),
            amount = (int)p.Amount,
            status = p.Status.ToString().ToLower(),
            time = GetTimeAgo(p.CreatedAt, now)
        }).ToList();



        var totalTransactions = await dbContext.Payments.CountAsync(ct);
        var completedCount = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid, ct);
        var pendingCount = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Pending, ct);
        var failedCount = await dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Failed || p.Status == PaymentStatus.Cancelled, ct);

        return Results.Ok(new
        {
            stats = new[]
            {
                new { label = "Total Transactions", value = totalTransactions.ToString("N0") },
                new { label = "Completed", value = completedCount.ToString("N0") },
                new { label = "Pending", value = pendingCount.ToString("N0") },
                new { label = "Failed", value = failedCount.ToString("N0") }
            },
            transactions,
            activity,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = true },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    public sealed record CreatePackageRequest(
        string Name,
        string Description,
        decimal Price,
        int Credits,
        string Color,
        List<string> Features,
        bool IsPopular,
        int Popularity);

    public sealed record UpdatePackageRequest(
        string Name,
        string Description,
        decimal Price,
        int Credits,
        string Color,
        List<string> Features,
        bool IsPopular,
        int Popularity,
        string Status);

    private static async Task<IResult> GetPackagesData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var dbPacks = await dbContext.ConnectPacks.ToListAsync(ct);

        var packageStats = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid && p.PackId != null)
            .GroupBy(p => p.PackId)
            .Select(g => new
            {
                PackId = g.Key ?? string.Empty,
                Sales = g.Count(),
                Revenue = g.Sum(p => p.Amount)
            })
            .ToDictionaryAsync(x => x.PackId, x => x, ct);

        var totalSales = packageStats.Values.Sum(x => x.Sales);
        var totalPackageRevenue = packageStats.Values.Sum(x => x.Revenue);

        var packages = dbPacks.Select(p =>
        {
            packageStats.TryGetValue(p.Code, out var statsVal);
            var salesCount = statsVal?.Sales ?? 0;
            var revenueAmount = statsVal?.Revenue ?? 0;
            return new
            {
                id = p.IntId,
                name = p.Name,
                description = p.Description,
                price = p.Price,
                credits = p.Credits,
                perCredit = p.Credits > 0 ? (double)(p.Price / p.Credits) : 0.0,
                color = p.Color,
                features = p.Features,
                sales = salesCount,
                revenue = (int)revenueAmount,
                popularity = totalSales > 0 ? (int)Math.Round((double)salesCount / totalSales * 100) : 0,
                status = p.Status,
                isPopular = p.IsPopular
            };
        }).ToList();

        var analytics = dbPacks.Where(p => p.Status == "active").Select(p =>
        {
            packageStats.TryGetValue(p.Code, out var statsVal);
            var salesCount = statsVal?.Sales ?? 0;
            var revenueAmount = statsVal?.Revenue ?? 0;
            return new
            {
                id = p.IntId,
                name = p.Name,
                credits = p.Credits,
                price = p.Price,
                sales = salesCount,
                revenue = (int)revenueAmount,
                conversion = totalSales > 0 ? (int)Math.Round((double)salesCount / totalSales * 100) : 0,
                status = p.Status,
                tag = p.IsPopular ? "Popular choice" : null
            };
        }).ToList();

        var activePackagesCount = dbPacks.Count(p => p.Status == "active");
        var avgPackageValue = dbPacks.Any() ? dbPacks.Average(p => (double)p.Price) : 0.0;

        return Results.Ok(new
        {
            stats = new[]
            {
                new { label = "Total Package Revenue", value = $"{totalPackageRevenue:N0} DA", icon = "dollar", color = "blue" },
                new { label = "Active Packages", value = activePackagesCount.ToString(), icon = "users", color = "purple" },
                new { label = "Total Sales", value = totalSales.ToString("N0"), icon = "trendingUp", color = "pink" },
                new { label = "Avg Package Value", value = $"{avgPackageValue:F0} DA", icon = "trendingUp", color = "green" }
            },
            packages,
            analytics,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = true },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    private static async Task<IResult> CreatePackage(
        [Microsoft.AspNetCore.Mvc.FromBody] CreatePackageRequest request,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required." });
        }

        var baseCode = request.Name.Trim().ToLowerInvariant().Replace(" ", "-");
        var code = baseCode;
        int suffix = 1;
        while (await dbContext.ConnectPacks.AnyAsync(p => p.Code == code, ct))
        {
            code = $"{baseCode}-{suffix}";
            suffix++;
        }

        var newPack = SP.Domain.Connects.ConnectPack.Create(
            code,
            request.Name,
            request.Description,
            request.Price,
            request.Credits,
            request.Color,
            request.Features,
            request.IsPopular,
            request.Popularity,
            "active"
        );

        dbContext.ConnectPacks.Add(newPack);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            id = newPack.IntId,
            name = newPack.Name,
            description = newPack.Description,
            price = newPack.Price,
            credits = newPack.Credits,
            perCredit = newPack.Credits > 0 ? (double)(newPack.Price / newPack.Credits) : 0.0,
            color = newPack.Color,
            features = newPack.Features,
            sales = 0,
            revenue = 0,
            popularity = newPack.Popularity,
            status = newPack.Status,
            isPopular = newPack.IsPopular
        });
    }

    private static async Task<IResult> UpdatePackage(
        int id,
        [Microsoft.AspNetCore.Mvc.FromBody] UpdatePackageRequest request,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var pack = await dbContext.ConnectPacks.FirstOrDefaultAsync(p => p.IntId == id, ct);
        if (pack is null)
        {
            return Results.NotFound();
        }

        pack.Update(
            request.Name,
            request.Description,
            request.Price,
            request.Credits,
            request.Color,
            request.Features,
            request.IsPopular,
            request.Popularity,
            request.Status
        );

        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            id = pack.IntId,
            name = pack.Name,
            description = pack.Description,
            price = pack.Price,
            credits = pack.Credits,
            perCredit = pack.Credits > 0 ? (double)(pack.Price / pack.Credits) : 0.0,
            color = pack.Color,
            features = pack.Features,
            sales = 0,
            revenue = 0,
            popularity = pack.Popularity,
            status = pack.Status,
            isPopular = pack.IsPopular
        });
    }

    private static async Task<IResult> DeletePackage(
        int id,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var pack = await dbContext.ConnectPacks.FirstOrDefaultAsync(p => p.IntId == id, ct);
        if (pack is null)
        {
            return Results.NotFound();
        }

        dbContext.ConnectPacks.Remove(pack);
        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<IResult> TogglePackageStatus(
        int id,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var pack = await dbContext.ConnectPacks.FirstOrDefaultAsync(p => p.IntId == id, ct);
        if (pack is null)
        {
            return Results.NotFound();
        }

        var newStatus = pack.Status == "active" ? "inactive" : "active";
        pack.UpdateStatus(newStatus);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new { status = newStatus });
    }

    private static async Task<IResult> GetAnalyticsData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var totalUsers = await dbContext.Users.CountAsync(ct);
        var totalRevenueAmount = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.Amount, ct);

        var totalRevStr = totalRevenueAmount > 0 
            ? $"{totalRevenueAmount / 1000:F0}K DA" 
            : "0K DA";
        
        var activeUsersStr = totalUsers.ToString("N0");

        var revenueThisMonth = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfMonth).SumAsync(p => p.Amount, ct);
        var revenueLastMonth = await dbContext.Payments.Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfMonth).SumAsync(p => p.Amount, ct);
        var trendRevenue = GetPercentageChange(revenueThisMonth, revenueLastMonth);

        var usersThisMonth = await dbContext.Users.CountAsync(u => u.CreatedAt >= startOfMonth, ct);
        var usersBeforeThisMonth = await dbContext.Users.CountAsync(u => u.CreatedAt < startOfMonth, ct);
        var trendUsers = GetPercentageChange(usersThisMonth, usersBeforeThisMonth);

        // Average Response Time
        var responseTimes = await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Accepted || b.Status == BookingStatus.Completed)
            .Where(b => b.ScheduledAt != null)
            .Select(b => ((b.ScheduledAt ?? b.CreatedAt) - b.CreatedAt).TotalHours)
            .ToListAsync(ct);

        var avgResponseTime = responseTimes.Any() ? responseTimes.Average() : 2.4;
        var responseTimeStr = $"{avgResponseTime:F1}h";

        var usersGrowthRate = usersBeforeThisMonth > 0 ? ((double)usersThisMonth / usersBeforeThisMonth * 100) : 0.0;

        // Categories performance
        var dbCategories = await dbContext.Categories.Take(5).ToListAsync(ct);
        var categoryPerformance = await dbContext.Bookings
            .Join(dbContext.Services, b => b.ServiceId, s => s.Id, (b, s) => new { b, s })
            .Join(dbContext.Categories, x => x.s.CategoryId, c => c.Id, (x, c) => new { c.Name, CategoryId = c.Id, Price = x.s.Price, Completed = x.b.Status == BookingStatus.Completed })
            .GroupBy(x => new { x.CategoryId, x.Name })
            .Select(g => new
            {
                name = g.Key.Name,
                requests = g.Count(),
                revenue = (int)g.Where(x => x.Completed).Sum(x => x.Price)
            })
            .OrderByDescending(x => x.requests)
            .Take(5)
            .ToListAsync(ct);

        var categoryPerformanceList = categoryPerformance.Select(x => new
        {
            name = x.name,
            requests = x.requests,
            revenue = x.revenue
        }).ToList();

        if (!categoryPerformanceList.Any())
        {
            categoryPerformanceList = dbCategories.Select(cat => new
            {
                name = cat.Name,
                requests = 0,
                revenue = 0
            }).ToList();
        }



        // Top Providers Rank
        var providerCompletions = await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .GroupBy(b => b.ProviderId)
            .Select(g => new { ProviderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProviderId, x => x.Count, ct);

        var providersList = await dbContext.Users
            .Where(u => u.Role == UserRole.Provider)
            .ToListAsync(ct);

        var providerRevenues = await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Join(dbContext.Services, b => b.ServiceId, s => s.Id, (b, s) => new { b.ProviderId, s.Price })
            .GroupBy(x => x.ProviderId)
            .Select(g => new { ProviderId = g.Key, Revenue = g.Sum(x => x.Price) })
            .ToDictionaryAsync(x => x.ProviderId, x => x.Revenue, ct);

        var providerRatings = await dbContext.Bookings
            .Where(b => b.Review != null)
            .GroupBy(b => b.ProviderId)
            .Select(g => new { ProviderId = g.Key, AvgRating = g.Average(b => b.Review!.Rating) })
            .ToDictionaryAsync(x => x.ProviderId, x => x.AvgRating, ct);

        var topProvidersList = providersList.Select(p => new
        {
            rank = 1,
            name = $"{p.FirstName} {p.LastName}",
            completions = providerCompletions.TryGetValue(p.Id, out var comp) ? comp : 0,
            rating = providerRatings.TryGetValue(p.Id, out var rat) ? Math.Round(rat, 1) : 5.0,
            revenue = (int)(providerRevenues.TryGetValue(p.Id, out var rev) ? rev : 0)
        })
        .OrderByDescending(x => x.revenue)
        .ThenByDescending(x => x.completions)
        .Take(5)
        .Select((x, idx) => new
        {
            rank = idx + 1,
            name = x.name,
            completions = x.completions,
            rating = x.rating,
            revenue = x.revenue
        })
        .ToList();



        // Growth Trends (last 5 months)
        var monthsList = Enumerable.Range(0, 5).Select(i => now.AddMonths(-4 + i)).ToList();
        var growthTrends = new List<object>();
        foreach (var m in monthsList)
        {
            var startOfM = new DateTime(m.Year, m.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfM = startOfM.AddMonths(1);

            var uCount = await dbContext.Users.CountAsync(u => u.CreatedAt >= startOfM && u.CreatedAt < endOfM, ct);
            var pCount = await dbContext.Users.CountAsync(u => u.Role == UserRole.Provider && u.CreatedAt >= startOfM && u.CreatedAt < endOfM, ct);
            var rCount = await dbContext.Bookings.CountAsync(b => b.CreatedAt >= startOfM && b.CreatedAt < endOfM, ct);

            growthTrends.Add(new
            {
                month = m.ToString("MMM"),
                users = uCount,
                providers = pCount,
                requests = rCount
            });
        }

        // Daily hourly activity clocks
        var dailyActivityList = await dbContext.Bookings
            .GroupBy(b => b.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Hour, x => x.Count, ct);

        var targetHours = new[] { 0, 4, 8, 12, 16, 20, 23 };
        var dailyActivity = targetHours.Select(h =>
        {
            var val = dailyActivityList.Where(x => Math.Abs(x.Key - h) <= 2).Sum(x => x.Value);
            return new
            {
                time = h.ToString("D2") + ":00",
                value = val
            };
        }).ToList();



        return Results.Ok(new
        {
            stats = new[]
            {
                new { label = "Total Revenue", value = totalRevStr, trend = trendRevenue, trendLabel = "from last month", icon = "dollar", color = "green", positive = true },
                new { label = "Active Users", value = activeUsersStr, trend = trendUsers, trendLabel = "from last month", icon = "users", color = "blue", positive = true },
                new { label = "Avg Response Time", value = responseTimeStr, trend = "-0%", trendLabel = "from last month", icon = "clock", color = "orange", positive = true },
                new { label = "Growth Rate", value = $"{usersGrowthRate:F0}%", trend = "+0%", trendLabel = "from last month", icon = "trendingUp", color = "purple", positive = true }
            },
            growthTrends,
            categoryPerformance = categoryPerformanceList,
            dailyActivity,
            topProviders = topProvidersList,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = true },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    private static async Task<IResult> GetUsersData(
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        ApplicationDbContext dbContext = null!,
        CancellationToken ct = default)
    {
        var query = dbContext.Users.AsQueryable();

        // Default to clients only, unless explicitly specified
        if (!string.IsNullOrEmpty(role))
        {
            if (role.ToLower() == "provider")
                query = query.Where(u => u.Role == UserRole.Provider);
            else
                query = query.Where(u => u.Role == UserRole.Client);
        }
        else
        {
            query = query.Where(u => u.Role == UserRole.Client);
        }

        if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
        {
            var stLower = status.ToLower();
            if (stLower == "blocked")
                query = query.Where(u => u.IsBlocked);
            else if (stLower == "pending")
                query = query.Where(u => !u.IsEmailVerified && !u.IsBlocked);
            else if (stLower == "active")
                query = query.Where(u => !u.IsBlocked);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => 
                u.FirstName.ToLower().Contains(searchLower) || 
                u.LastName.ToLower().Contains(searchLower) || 
                u.Email.ToLower().Contains(searchLower) ||
                (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(ct);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;

        var dbUsers = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = dbUsers.Select(u => u.Id).ToList();

        // Get requests count (bookings count) for each user
        var requestsCounts = await dbContext.Bookings
            .Where(b => userIds.Contains(b.ClientId))
            .GroupBy(b => b.ClientId)
            .Select(g => new { ClientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientId, x => x.Count, ct);

        int startIndex = (page - 1) * pageSize + 1;
        var clients = dbUsers.Select((u, index) => new
        {
            id = startIndex + index,
            userId = u.Id.ToString(),
            name = $"{u.FirstName} {u.LastName}",
            email = u.Email,
            phone = u.PhoneNumber ?? "",
            initials = $"{(u.FirstName.Length > 0 ? u.FirstName[0].ToString() : "")}{(u.LastName.Length > 0 ? u.LastName[0].ToString() : "")}".ToUpper(),
            status = u.IsBlocked ? "blocked" : (!u.IsEmailVerified ? "pending" : "active"),
            joined = u.CreatedAt.ToString("yyyy-MM-dd"),
            requests = requestsCounts.TryGetValue(u.Id, out var reqs) ? reqs : 0
        }).ToList();

        return Results.Ok(new
        {
            clients,
            users = clients,
            totalCount,
            page,
            pageSize,
            totalPages,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = true },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    private static async Task<IResult> GetProvidersData(
        [FromQuery] string? status,
        [FromQuery] string? category,
        [FromQuery] string? search,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var query = dbContext.Users.Where(u => u.Role == UserRole.Provider).AsQueryable();

        // Get providers services to resolve categories
        var providerServices = await dbContext.Services.ToListAsync(ct);
        var categories = await dbContext.Categories.ToListAsync(ct);
        var categoriesMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var providerToCategoryMap = providerServices
            .GroupBy(s => s.ProviderId)
            .ToDictionary(
                g => g.Key,
                g => categoriesMap.TryGetValue(g.First().CategoryId, out var name) ? name : "Uncategorized"
            );

        if (!string.IsNullOrEmpty(category))
        {
            var catLower = category.ToLower();
            var matchedCategoryIds = categories
                .Where(c => c.Name.ToLower().Contains(catLower))
                .Select(c => c.Id)
                .ToList();

            var providerIdsInCat = providerServices
                .Where(s => matchedCategoryIds.Contains(s.CategoryId))
                .Select(s => s.ProviderId)
                .Distinct()
                .ToList();

            query = query.Where(u => providerIdsInCat.Contains(u.Id));
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "pending")
                query = query.Where(u => !u.IsEmailVerified);
            else if (status == "approved")
                query = query.Where(u => u.IsEmailVerified && !u.IsBlocked);
            else if (status == "rejected")
                query = query.Where(u => u.IsBlocked);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => 
                u.FirstName.ToLower().Contains(searchLower) || 
                u.LastName.ToLower().Contains(searchLower) || 
                u.Email.ToLower().Contains(searchLower));
        }

        var dbProviders = await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);

        var providerIds = dbProviders.Select(p => p.Id).ToList();

        // Fetch completed bookings count per provider
        var completedCounts = await dbContext.Bookings
            .Where(b => providerIds.Contains(b.ProviderId) && b.Status == BookingStatus.Completed)
            .GroupBy(b => b.ProviderId)
            .Select(g => new { ProviderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProviderId, x => x.Count, ct);

        // Fetch ratings per provider
        var ratings = await dbContext.Bookings
            .Where(b => providerIds.Contains(b.ProviderId) && b.Review != null)
            .GroupBy(b => b.ProviderId)
            .Select(g => new { ProviderId = g.Key, AvgRating = g.Average(b => b.Review!.Rating) })
            .ToDictionaryAsync(x => x.ProviderId, x => x.AvgRating, ct);

        // Fetch wallets for credits balance
        var wallets = await dbContext.Wallets
            .Where(w => providerIds.Contains(w.UserId))
            .ToDictionaryAsync(w => w.UserId, w => w.Balance, ct);

        // Fetch connect transactions (total credits purchased)
        var totalPurchasedCredits = await dbContext.ConnectTransactions
            .Where(t => providerIds.Contains(t.UserId) && t.Type == "Purchase")
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total, ct);

        // Fetch payments for total amount spent and last purchase date
        var payments = await dbContext.Payments
            .Where(p => providerIds.Contains(p.UserId) && p.Status == PaymentStatus.Paid)
            .ToListAsync(ct);

        var totalSpentMap = payments
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        var lastSpentMap = payments
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Max(p => p.CreatedAt));

        int localId = 1;
        var providers = dbProviders.Select(p => new
        {
            id = localId++,
            name = $"{p.FirstName} {p.LastName}",
            email = p.Email,
            initials = $"{(p.FirstName.Length > 0 ? p.FirstName[0].ToString() : "")}{(p.LastName.Length > 0 ? p.LastName[0].ToString() : "")}".ToUpper(),
            verified = p.IsEmailVerified,
            category = providerToCategoryMap.TryGetValue(p.Id, out var catName) ? catName : "Home Services",
            status = p.IsBlocked ? "rejected" : (p.IsEmailVerified ? "approved" : "pending"),
            pendingSince = p.IsEmailVerified ? null : "Registered: " + p.CreatedAt.ToString("yyyy-MM-dd"),
            rating = ratings.TryGetValue(p.Id, out var r) ? (double?)Math.Round(r, 1) : null,
            completed = completedCounts.TryGetValue(p.Id, out var cCount) ? cCount : 0,
            credits = wallets.TryGetValue(p.Id, out var balance) ? balance : 0,
            creditsTotal = totalPurchasedCredits.TryGetValue(p.Id, out var totalC) ? totalC : 0,
            totalSpent = totalSpentMap.TryGetValue(p.Id, out var spent) ? (int)spent : 0,
            lastSpent = lastSpentMap.TryGetValue(p.Id, out var lastSp) ? lastSp.ToString("yyyy-MM-dd") : null,
            connectPermission = true
        }).ToList();

        var pendingCount = dbProviders.Count(p => p.IsBlocked == false && p.IsEmailVerified == false);
        var activeCount = dbProviders.Count(p => p.IsEmailVerified);
        var allRatings = ratings.Values.ToList();
        var avgRating = allRatings.Any() ? Math.Round(allRatings.Average(), 1) : 4.5;

        var stats = new[]
        {
            new { label = "Pending Approval", value = pendingCount.ToString(), icon = "pending", color = "orange" },
            new { label = "Active Providers", value = activeCount.ToString("N0"), icon = "active", color = "green" },
            new { label = "Average Rating", value = avgRating.ToString("F1"), icon = "rating", color = "yellow" }
        };

        return Results.Ok(new
        {
            stats,
            providers,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = true },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = false },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    private static async Task<IResult> GetCategoriesData(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var dbCats = await dbContext.Categories.ToListAsync(ct);
        var services = await dbContext.Services.ToListAsync(ct);
        var bookings = await dbContext.Bookings.Include(b => b.Review).ToListAsync(ct);

        var providerCountPerCat = services
            .GroupBy(s => s.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.ProviderId).Distinct().Count());

        var categoryBookings = dbCats.ToDictionary(
            c => c.Id,
            c => bookings.Join(services.Where(s => s.CategoryId == c.Id), b => b.ServiceId, s => s.Id, (b, s) => b).ToList()
        );

        var colors = new[] { "bg-indigo-500", "bg-blue-500", "bg-pink-500", "bg-orange-500", "bg-emerald-500", "bg-purple-500" };
        var icons = new[] { "home", "wrench", "heart", "graduation", "car", "settings" };

        int localId = 1;
        var categories = dbCats.Select((c, idx) =>
        {
            categoryBookings.TryGetValue(c.Id, out var catB);
            var totalBookingsCount = catB?.Count ?? 0;
            var completedCount = catB?.Count(b => b.Status == BookingStatus.Completed) ?? 0;
            var reviews = catB?.Where(b => b.Review != null).Select(b => b.Review!.Rating).ToList();

            var completionRate = totalBookingsCount > 0 
                ? (int)Math.Round((double)completedCount / totalBookingsCount * 100) 
                : 100;

            var avgRating = reviews != null && reviews.Any() 
                ? Math.Round(reviews.Average(), 1) 
                : 5.0;

            return new
            {
                id = c.Id.ToString(),
                localId = localId++,
                name = c.Name,
                description = c.Description,
                icon = icons[idx % icons.Length],
                color = colors[idx % colors.Length],
                providers = providerCountPerCat.TryGetValue(c.Id, out var providers) ? providers : 0,
                requests = totalBookingsCount,
                rating = avgRating,
                completionRate = completionRate,
                growth = "+0%"
            };
        }).ToList();

        var totalCategories = dbCats.Count;
        var totalProviders = await dbContext.Users.CountAsync(u => u.Role == UserRole.Provider, ct);
        var totalRequests = bookings.Count;

        var summaryStats = new[]
        {
            new { label = "Total Categories", value = totalCategories.ToString() },
            new { label = "Total Providers", value = totalProviders.ToString("N0") },
            new { label = "Total Requests", value = totalRequests.ToString("N0") }
        };

        return Results.Ok(new
        {
            summaryStats,
            categories,
            navItems = new[]
            {
                new { label = "Dashboard", icon = "dashboard", active = false },
                new { label = "Clients", icon = "users", active = false },
                new { label = "Providers", icon = "providers", active = false },
                new { label = "Messaging", icon = "messaging", active = false },
                new { label = "Categories", icon = "categories", active = true },
                new { label = "Revenue", icon = "revenue", active = false },
                new { label = "Transactions", icon = "transactions", active = false },
                new { label = "Packages", icon = "packages", active = false },
                new { label = "Analytics", icon = "analytics", active = false },
                new { label = "Settings", icon = "settings", active = false }
            }
        });
    }

    public sealed record AdminCreateCategoryRequest(string Name, string Description);

    private static async Task<IResult> CreateCategory(
        [FromBody] AdminCreateCategoryRequest request,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Category name is required.");

        var category = SP.Domain.Categories.Category.Create(request.Name, request.Description);

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new { id = category.Id });
    }

    private static async Task<IResult> DeleteCategory(
        string id,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        Category? category = null;
        if (Guid.TryParse(id, out var guidId))
        {
            category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == guidId, ct);
        }

        if (category is null)
        {
            var allCats = await dbContext.Categories.ToListAsync(ct);
            if (int.TryParse(id, out var intId) && intId > 0 && intId <= allCats.Count)
            {
                category = allCats[intId - 1];
            }
            else
            {
                category = allCats.FirstOrDefault(c => c.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (category is null)
        {
            return Results.NotFound(new { message = "Category not found." });
        }

        // Clean up linked services and bookings to maintain database integrity
        var services = await dbContext.Services.Where(s => s.CategoryId == category.Id).ToListAsync(ct);
        if (services.Any())
        {
            var serviceIds = services.Select(s => s.Id).ToList();
            var bookings = await dbContext.Bookings.Where(b => serviceIds.Contains(b.ServiceId)).ToListAsync(ct);
            if (bookings.Any())
            {
                dbContext.Bookings.RemoveRange(bookings);
            }

            dbContext.Services.RemoveRange(services);
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleConversationLock(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var conversation = await dbContext.Conversations.FindAsync(new object[] { id }, ct);
        if (conversation is null)
            return Results.NotFound("Conversation not found.");

        if (conversation.IsLocked)
            conversation.Unlock();
        else
        {
            dbContext.Entry(conversation).Property(c => c.IsLocked).CurrentValue = true;
        }

        await dbContext.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ResolveReport(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .Include(b => b.Report)
            .FirstOrDefaultAsync(b => b.Report != null && b.Report.Id == id, ct);

        if (booking is null || booking.Report is null)
            return Results.NotFound("Report not found.");

        booking.ResolveReport();
        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}

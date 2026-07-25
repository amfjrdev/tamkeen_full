using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SP.IntegrationTests.Common;
using Xunit;

namespace SP.IntegrationTests.Endpoints;

/// <summary>
/// End-to-end test simulating the full booking lifecycle:
/// Client registers → Provider registers → Provider creates service →
/// Client books → Provider accepts → Both chat → Provider completes →
/// Client reviews → Client reports
/// </summary>
public sealed class BookingLifecycleTests : BaseIntegrationTest
{
    public BookingLifecycleTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task FullBookingLifecycle_CompletesSuccessfully()
    {
        // ── 1. Register client and provider ───────────────────────────────
        var clientToken = await RegisterAndLoginAsync("client@example.com", "Test@1234!", "Client");
        var providerToken = await RegisterAndLoginAsync("provider@example.com", "Test@1234!", "Provider");

        // ── 2. Admin creates a category ───────────────────────────────────
        var adminToken = await RegisterAndLoginAsync("admin@example.com", "Test@1234!", "Admin");
        SetAuthToken(adminToken);

        var catResp = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = "Plumbing",
            description = "Plumbing services"
        });
        catResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var catBody = await ReadAsync<IdResponse>(catResp);
        var categoryId = catBody!.Id;

        // ── 3. Provider creates a service ─────────────────────────────────
        SetAuthToken(providerToken);

        var svcResp = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId,
            name = "Pipe Repair",
            description = "Fix leaking pipes",
            price = 75.00,
            durationMinutes = 60
        });
        svcResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var svcBody = await ReadAsync<IdResponse>(svcResp);
        var serviceId = svcBody!.Id;

        // ── 4. Client creates a booking ───────────────────────────────────
        SetAuthToken(clientToken);

        var bookingResp = await Client.PostAsJsonAsync("/api/bookings", new
        {
            serviceId,
            scheduledDate = DateTime.UtcNow.AddDays(5)
        });
        bookingResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var bookingBody = await ReadAsync<IdResponse>(bookingResp);
        var bookingId = bookingBody!.Id;

        // ── 5. Provider accepts the booking ───────────────────────────────
        SetAuthToken(providerToken);

        var acceptResp = await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/accept", new { });
        acceptResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ── 6. Verify booking is now Accepted ─────────────────────────────
        SetAuthToken(clientToken);

        var getResp = await Client.GetAsync($"/api/bookings/{bookingId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var booking = await ReadAsync<BookingResponse>(getResp);
        booking!.Status.Should().Be("Accepted");

        // ── 7. Provider completes the booking ─────────────────────────────
        SetAuthToken(providerToken);

        var completeResp = await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/complete", new { });
        completeResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ── 8. Client adds a review ───────────────────────────────────────
        SetAuthToken(clientToken);

        var reviewResp = await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/review", new
        {
            rating = 5,
            comment = "Excellent service!"
        });
        reviewResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ── 9. Client adds a report ───────────────────────────────────────
        var reportResp = await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/report", new
        {
            reason = "Provider arrived late"
        });
        reportResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ── 10. Verify final state ────────────────────────────────────────
        var finalResp = await Client.GetAsync($"/api/bookings/{bookingId}");
        var finalBooking = await ReadAsync<BookingDetailResponse>(finalResp);
        finalBooking!.Status.Should().Be("Completed");
        finalBooking.Review.Should().NotBeNull();
        finalBooking.Review!.Rating.Should().Be(5);
        finalBooking.Report.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBooking_WithInactiveService_Returns400()
    {
        var providerToken = await RegisterAndLoginAsync("p2@example.com", "Test@1234!", "Provider");
        var clientToken = await RegisterAndLoginAsync("c2@example.com", "Test@1234!", "Client");
        var adminToken = await RegisterAndLoginAsync("a2@example.com", "Test@1234!", "Admin");

        SetAuthToken(adminToken);
        var catResp = await Client.PostAsJsonAsync("/api/categories", new { name = "Electrical", description = "desc" });
        var catId = (await ReadAsync<IdResponse>(catResp))!.Id;

        SetAuthToken(providerToken);
        var svcResp = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId = catId,
            name = "Wiring",
            description = "desc",
            price = 100.0,
            durationMinutes = 90
        });
        var svcId = (await ReadAsync<IdResponse>(svcResp))!.Id;

        // Deactivate the service
        await Client.PatchAsync($"/api/services/{svcId}/deactivate", null);

        // Client tries to book deactivated service
        SetAuthToken(clientToken);
        var bookingResp = await Client.PostAsJsonAsync("/api/bookings", new
        {
            serviceId = svcId,
            scheduledDate = DateTime.UtcNow.AddDays(3)
        });

        bookingResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AcceptBooking_ByWrongProvider_Returns403()
    {
        var provider1Token = await RegisterAndLoginAsync("p3@example.com", "Test@1234!", "Provider");
        var provider2Token = await RegisterAndLoginAsync("p4@example.com", "Test@1234!", "Provider");
        var clientToken = await RegisterAndLoginAsync("c3@example.com", "Test@1234!", "Client");
        var adminToken = await RegisterAndLoginAsync("a3@example.com", "Test@1234!", "Admin");

        SetAuthToken(adminToken);
        var catResp = await Client.PostAsJsonAsync("/api/categories", new { name = "Cleaning", description = "desc" });
        var catId = (await ReadAsync<IdResponse>(catResp))!.Id;

        SetAuthToken(provider1Token);
        var svcResp = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId = catId,
            name = "House Cleaning",
            description = "desc",
            price = 50.0,
            durationMinutes = 120
        });
        var svcId = (await ReadAsync<IdResponse>(svcResp))!.Id;

        SetAuthToken(clientToken);
        var bookingResp = await Client.PostAsJsonAsync("/api/bookings", new
        {
            serviceId = svcId,
            scheduledDate = DateTime.UtcNow.AddDays(2)
        });
        var bookingId = (await ReadAsync<IdResponse>(bookingResp))!.Id;

        // Provider2 tries to accept Provider1's booking
        SetAuthToken(provider2Token);
        var acceptResp = await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/accept", new { });

        acceptResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CancelBooking_AfterCompletion_Returns409()
    {
        var providerToken = await RegisterAndLoginAsync("p5@example.com", "Test@1234!", "Provider");
        var clientToken = await RegisterAndLoginAsync("c5@example.com", "Test@1234!", "Client");
        var adminToken = await RegisterAndLoginAsync("a5@example.com", "Test@1234!", "Admin");

        SetAuthToken(adminToken);
        var catResp = await Client.PostAsJsonAsync("/api/categories", new { name = "Gardening", description = "desc" });
        var catId = (await ReadAsync<IdResponse>(catResp))!.Id;

        SetAuthToken(providerToken);
        var svcResp = await Client.PostAsJsonAsync("/api/services", new
        {
            categoryId = catId,
            name = "Lawn Mowing",
            description = "desc",
            price = 40.0,
            durationMinutes = 60
        });
        var svcId = (await ReadAsync<IdResponse>(svcResp))!.Id;

        SetAuthToken(clientToken);
        var bookingResp = await Client.PostAsJsonAsync("/api/bookings", new
        {
            serviceId = svcId,
            scheduledDate = DateTime.UtcNow.AddDays(2)
        });
        var bookingId = (await ReadAsync<IdResponse>(bookingResp))!.Id;

        SetAuthToken(providerToken);
        await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/accept", new { });
        await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/complete", new { });

        SetAuthToken(clientToken);
        var cancelResp = await Client.PostAsJsonAsync($"/api/bookings/{bookingId}/cancel", new { });

        cancelResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── Response models ───────────────────────────────────────────────────

    private sealed record IdResponse(Guid Id);

    private sealed record BookingResponse(Guid Id, string Status);

    private sealed record BookingDetailResponse(
        Guid Id,
        string Status,
        ReviewDto? Review,
        ReportDto? Report);

    private sealed record ReviewDto(int Rating, string Comment);
    private sealed record ReportDto(string Reason, string Status);
}

using System;
using System.Linq;
using FluentAssertions;
using SP.Domain.ServiceRequests;
using SP.Domain.ServiceRequests.Enums;
using SP.Domain.ServiceRequests.Events;
using Xunit;

namespace SP.UnitTests.Domain.ServiceRequests;

public class ServiceRequestTests
{
    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccessWithPendingAdminReview()
    {
        var clientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var result = ServiceRequest.Create(
            clientId,
            categoryId,
            "نحتاج plombier لإصلاح تسريب",
            "يوجد تسريب ماء تحت المغسلة بالمطبخ",
            "Constantine",
            1500);

        result.IsSuccess.Should().BeTrue();
        var request = result.Value;
        request.Status.Should().Be(ServiceRequestStatus.PendingAdminReview);
        request.Title.Should().Be("نحتاج plombier لإصلاح تسريب");
        request.Wilaya.Should().Be("Constantine");
        request.Budget.Should().Be(1500);
        request.DomainEvents.Should().ContainSingle(e => e is ServiceRequestCreatedEvent);
    }

    [Fact]
    public void Approve_WhenPendingReview_ShouldTransitionToApproved()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            "Constantine").Value;

        var result = request.Approve();

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Approved);
        request.ApprovedAt.Should().NotBeNull();
        request.DomainEvents.Should().Contain(e => e is ServiceRequestApprovedEvent);
    }

    [Fact]
    public void AddApplication_MatchingWilayaAndApproved_ShouldAddApplication()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            "Constantine").Value;
        request.Approve();

        var providerId = Guid.NewGuid();
        var result = request.AddApplication(
            providerId,
            "Constantine",
            "I can fix this plumbing issue today.",
            2000,
            10);

        result.IsSuccess.Should().BeTrue();
        request.Applications.Should().HaveCount(1);
        request.Applications.First().ProviderId.Should().Be(providerId);
        request.Applications.First().Status.Should().Be(RequestApplicationStatus.Pending);
    }

    [Fact]
    public void AddApplication_MismatchedWilaya_ShouldFail()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            "Constantine").Value;
        request.Approve();

        var providerId = Guid.NewGuid();
        var result = request.AddApplication(
            providerId,
            "Algiers",
            "I can travel from Algiers.",
            2000,
            10);

        result.IsFailure.Should().BeTrue();
        request.Applications.Should().BeEmpty();
    }

    [Fact]
    public void SelectProvider_ValidApplication_ShouldTransitionToProviderSelectedAndAcceptApplication()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            "Constantine").Value;
        request.Approve();

        var provider1 = Guid.NewGuid();
        var provider2 = Guid.NewGuid();
        var app1 = request.AddApplication(provider1, "Constantine", "Letter 1", 1000).Value;
        var app2 = request.AddApplication(provider2, "Constantine", "Letter 2", 1200).Value;

        var result = request.SelectProvider(provider1, app1.Id);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.ProviderSelected);
        request.SelectedProviderId.Should().Be(provider1);
        request.SelectedApplicationId.Should().Be(app1.Id);

        app1.Status.Should().Be(RequestApplicationStatus.Accepted);
        app2.Status.Should().Be(RequestApplicationStatus.Rejected);
    }

    [Fact]
    public void CompleteAndReview_ValidFlow_ShouldSucceed()
    {
        var clientId = Guid.NewGuid();
        var request = ServiceRequest.Create(
            clientId,
            Guid.NewGuid(),
            "Title",
            "Description",
            "Constantine").Value;
        request.Approve();

        var providerId = Guid.NewGuid();
        var app = request.AddApplication(providerId, "Constantine", "Letter", 1000).Value;
        request.SelectProvider(providerId, app.Id);

        var completeResult = request.Complete();
        completeResult.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Completed);
        request.CompletedAt.Should().NotBeNull();

        var reviewResult = request.AddReview(clientId, 5, "Excellent work!");
        reviewResult.IsSuccess.Should().BeTrue();
        request.Review.Should().NotBeNull();
        request.Review!.Rating.Should().Be(5);
        request.Review.Comment.Should().Be("Excellent work!");

        // Duplicate review should fail
        var duplicateReview = request.AddReview(clientId, 4, "Another review");
        duplicateReview.IsFailure.Should().BeTrue();
    }
}

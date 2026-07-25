using FluentAssertions;
using SP.Domain.Services;
using SP.Domain.Services.Errors;
using Xunit;

namespace SP.UnitTests.Domain;

public sealed class ServiceAggregateTests
{
    private static readonly Guid ProviderId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsActiveService()
    {
        var result = Service.Create(ProviderId, CategoryId, "Haircut", "Professional haircut", 25.00m, 30);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
        result.Value.Name.Should().Be("Haircut");
        result.Value.DomainEvents.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsFailure(string name)
    {
        var result = Service.Create(ProviderId, CategoryId, name, "desc", 25m, 30);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidServiceName);
    }

    [Fact]
    public void Create_WithNameExceeding100Chars_ReturnsFailure()
    {
        var result = Service.Create(ProviderId, CategoryId, new string('A', 101), "desc", 25m, 30);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidServiceName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativePrice_ReturnsFailure(decimal price)
    {
        var result = Service.Create(ProviderId, CategoryId, "Haircut", "desc", price, 30);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithZeroOrNegativeDuration_ReturnsFailure(int duration)
    {
        var result = Service.Create(ProviderId, CategoryId, "Haircut", "desc", 25m, duration);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidDuration);
    }

    [Fact]
    public void Create_WithEmptyProviderId_ReturnsFailure()
    {
        var result = Service.Create(Guid.Empty, CategoryId, "Haircut", "desc", 25m, 30);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidProviderId);
    }

    [Fact]
    public void Deactivate_WhenActive_ReturnsSuccess()
    {
        var service = CreateValidService();

        var result = service.Deactivate();

        result.IsSuccess.Should().BeTrue();
        service.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        var service = CreateValidService();
        service.Deactivate();

        var result = service.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.ServiceAlreadyInactive);
    }

    [Fact]
    public void Activate_WhenInactive_ReturnsSuccess()
    {
        var service = CreateValidService();
        service.Deactivate();

        var result = service.Activate();

        result.IsSuccess.Should().BeTrue();
        service.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsFailure()
    {
        var service = CreateValidService();

        var result = service.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.ServiceAlreadyActive);
    }

    [Fact]
    public void EnsureCanBeBooked_WhenActive_ReturnsSuccess()
    {
        var service = CreateValidService();

        var result = service.EnsureCanBeBooked();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void EnsureCanBeBooked_WhenInactive_ReturnsFailure()
    {
        var service = CreateValidService();
        service.Deactivate();

        var result = service.EnsureCanBeBooked();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.ServiceUnavailable);
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ReturnsSuccess()
    {
        var service = CreateValidService();

        var result = service.UpdatePrice(50m);

        result.IsSuccess.Should().BeTrue();
        service.Price.Should().Be(50m);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ReturnsFailure()
    {
        var service = CreateValidService();

        var result = service.UpdatePrice(-10m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }

    private static Service CreateValidService()
        => Service.Create(ProviderId, CategoryId, "Haircut", "Professional haircut", 25m, 30).Value;
}

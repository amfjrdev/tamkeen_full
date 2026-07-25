using SP.Domain.Abstractions;
using SP.Domain.Services.Errors;
using SP.Domain.Services.Events;

namespace SP.Domain.Services;

public sealed class Service : AggregateRoot
{
    private Service() { }

    private Service(
        Guid id,
        Guid providerId,
        Guid categoryId,
        string name,
        string description,
        decimal price,
        int durationMinutes)
        : base(id)
    {
        ProviderId = providerId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ProviderId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Result<Service> Create(
        Guid providerId,
        Guid categoryId,
        string name,
        string description,
        decimal price,
        int durationMinutes)
    {
        if (providerId == Guid.Empty)
            return Result.Failure<Service>(ServiceErrors.InvalidProviderId);

        if (categoryId == Guid.Empty)
            return Result.Failure<Service>(ServiceErrors.InvalidCategoryId);

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return Result.Failure<Service>(ServiceErrors.InvalidServiceName);

        if (!string.IsNullOrEmpty(description) && description.Length > 500)
            return Result.Failure<Service>(ServiceErrors.InvalidServiceDescription);

        if (price <= 0)
            return Result.Failure<Service>(ServiceErrors.InvalidPrice);

        if (durationMinutes <= 0)
            return Result.Failure<Service>(ServiceErrors.InvalidDuration);

        var service = new Service(Guid.NewGuid(), providerId, categoryId, name.Trim(), description?.Trim() ?? string.Empty, price, durationMinutes);
        service.RaiseDomainEvent(new ServiceCreatedEvent(service.Id, service.ProviderId));
        return Result.Success(service);
    }

    public Result UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            return Result.Failure(ServiceErrors.InvalidPrice);

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateDuration(int newDurationMinutes)
    {
        if (newDurationMinutes <= 0)
            return Result.Failure(ServiceErrors.InvalidDuration);

        DurationMinutes = newDurationMinutes;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateDescription(string newDescription)
    {
        if (!string.IsNullOrEmpty(newDescription) && newDescription.Length > 500)
            return Result.Failure(ServiceErrors.InvalidServiceDescription);

        Description = newDescription?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(ServiceErrors.ServiceAlreadyActive);

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ServiceActivatedEvent(Id, ProviderId));
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(ServiceErrors.ServiceAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ServiceDeactivatedEvent(Id, ProviderId));
        return Result.Success();
    }

    public Result EnsureCanBeBooked()
    {
        if (!IsActive)
            return Result.Failure(ServiceErrors.ServiceUnavailable);

        return Result.Success();
    }
}

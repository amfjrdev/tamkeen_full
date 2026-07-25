using SP.Domain.Abstractions;
using System;
using System.Collections.Generic;

namespace SP.Domain.Connects;

public sealed class ConnectPack : Entity
{
    private ConnectPack() { }

    private ConnectPack(
        Guid id,
        string code,
        string name,
        string description,
        decimal price,
        int credits,
        string color,
        List<string> features,
        bool isPopular,
        int popularity,
        string status) : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        Price = price;
        Credits = credits;
        Color = color;
        Features = features;
        IsPopular = isPopular;
        Popularity = popularity;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public int IntId { get; private set; } // Auto-incremented identity column for legacy frontend compatibility
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Credits { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public List<string> Features { get; private set; } = new();
    public bool IsPopular { get; private set; }
    public int Popularity { get; private set; }
    public string Status { get; private set; } = "active"; // "active" or "inactive"
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static ConnectPack Create(
        string code,
        string name,
        string description,
        decimal price,
        int credits,
        string color,
        List<string> features,
        bool isPopular,
        int popularity,
        string status = "active")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new ConnectPack(
            Guid.NewGuid(),
            code.Trim().ToLowerInvariant(),
            name.Trim(),
            description?.Trim() ?? string.Empty,
            price,
            credits,
            color ?? "blue",
            features ?? new List<string>(),
            isPopular,
            popularity,
            status);
    }

    public void Update(
        string name,
        string description,
        decimal price,
        int credits,
        string color,
        List<string> features,
        bool isPopular,
        int popularity,
        string status)
    {
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Credits = credits;
        Color = color ?? "blue";
        Features = features ?? new List<string>();
        IsPopular = isPopular;
        Popularity = popularity;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}

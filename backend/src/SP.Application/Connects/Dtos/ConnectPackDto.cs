namespace SP.Application.Connects.Dtos;

public sealed record ConnectPackDto(
    string Id,
    string Name,
    int Connects,
    decimal Price,
    string Currency);

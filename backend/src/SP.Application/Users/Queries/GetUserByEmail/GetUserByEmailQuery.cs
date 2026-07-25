using SP.Application.Abstractions.Messaging;
using SP.Application.Users.Dto;

namespace SP.Application.Users.Queries.GetUserByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserResponseDto>;
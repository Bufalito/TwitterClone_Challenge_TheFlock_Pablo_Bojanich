namespace Application.DTOs;

public sealed record RegisterResponse
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}

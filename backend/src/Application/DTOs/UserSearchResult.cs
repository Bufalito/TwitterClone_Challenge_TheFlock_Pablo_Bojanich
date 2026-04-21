namespace Application.DTOs;

public record UserSearchResult(
    Guid Id,
    string Username,
    string DisplayName,
    string? Bio,
    string? Avatar,
    int FollowersCount
);

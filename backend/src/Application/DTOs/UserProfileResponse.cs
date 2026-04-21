namespace Application.DTOs;

public record UserProfileResponse(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    string? Bio,
    string? Avatar,
    int FollowersCount,
    int FollowingCount,
    int TweetsCount,
    DateTime CreatedAtUtc,
    bool IsFollowedByCurrentUser
);

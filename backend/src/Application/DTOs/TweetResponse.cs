namespace Application.DTOs;

public record TweetResponse(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAtUtc,
    string Username,
    string DisplayName,
    int LikesCount
);

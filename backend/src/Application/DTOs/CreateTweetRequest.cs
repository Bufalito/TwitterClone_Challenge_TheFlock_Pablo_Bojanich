namespace Application.DTOs;

public record CreateTweetRequest(string Content, Guid? ParentTweetId = null);

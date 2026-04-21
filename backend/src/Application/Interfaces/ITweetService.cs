using Application.DTOs;

namespace Application.Interfaces;

public interface ITweetService
{
    Task<TweetResponse> CreateAsync(Guid userId, CreateTweetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default);

    Task<List<TweetResponse>> GetRecentAsync(int count = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default);

    Task<List<TweetResponse>> GetByUserAsync(string username, Guid? currentUserId = null, CancellationToken cancellationToken = default);

    Task<List<TweetResponse>> GetTimelineAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task LikeAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default);

    Task UnlikeAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default);
}

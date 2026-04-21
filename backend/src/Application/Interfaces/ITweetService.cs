using Application.DTOs;

namespace Application.Interfaces;

public interface ITweetService
{
    Task<TweetResponse> CreateAsync(Guid userId, CreateTweetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default);

    Task<List<TweetResponse>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default);

    Task<List<TweetResponse>> GetByUserAsync(string username, CancellationToken cancellationToken = default);
}

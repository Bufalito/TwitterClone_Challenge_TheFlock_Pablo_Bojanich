using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class TweetService : ITweetService
{
    private readonly DbContext _context;

    public TweetService(DbContext context)
    {
        _context = context;
    }

    public async Task<TweetResponse> CreateAsync(Guid userId, CreateTweetRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var tweet = new Tweet(userId, request.Content);

        _context.Set<Tweet>().Add(tweet);
        await _context.SaveChangesAsync(cancellationToken);

        return new TweetResponse(
            tweet.Id,
            tweet.UserId,
            tweet.Content,
            tweet.CreatedAtUtc,
            user.Username,
            user.DisplayName,
            0,
            false
        );
    }

    public async Task DeleteAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default)
    {
        var tweet = await _context.Set<Tweet>()
            .FirstOrDefaultAsync(t => t.Id == tweetId, cancellationToken);

        if (tweet == null)
        {
            throw new InvalidOperationException($"Tweet with ID '{tweetId}' not found.");
        }

        if (tweet.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own tweets.");
        }

        _context.Set<Tweet>().Remove(tweet);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TweetResponse>> GetRecentAsync(int count = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var tweets = await _context.Set<Tweet>()
            .Include(t => t.User)
            .Include(t => t.Likes)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

        return tweets.Select(t => new TweetResponse(
            t.Id,
            t.UserId,
            t.Content,
            t.CreatedAtUtc,
            t.User.Username,
            t.User.DisplayName,
            t.Likes.Count,
            currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value)
        )).ToList();
    }

    public async Task<List<TweetResponse>> GetByUserAsync(string username, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var tweets = await _context.Set<Tweet>()
            .Include(t => t.User)
            .Include(t => t.Likes)
            .Where(t => t.User.Username == username)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return tweets.Select(t => new TweetResponse(
            t.Id,
            t.UserId,
            t.Content,
            t.CreatedAtUtc,
            t.User.Username,
            t.User.DisplayName,
            t.Likes.Count,
            currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value)
        )).ToList();
    }

    public async Task LikeAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default)
    {
        // Check if tweet exists
        var tweetExists = await _context.Set<Tweet>()
            .AnyAsync(t => t.Id == tweetId, cancellationToken);

        if (!tweetExists)
        {
            throw new InvalidOperationException($"Tweet with ID '{tweetId}' not found.");
        }

        // Check if like already exists
        var likeExists = await _context.Set<Like>()
            .AnyAsync(l => l.UserId == userId && l.TweetId == tweetId, cancellationToken);

        if (likeExists)
        {
            throw new InvalidOperationException("You have already liked this tweet.");
        }

        var like = new Like(userId, tweetId);
        _context.Set<Like>().Add(like);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlikeAsync(Guid userId, Guid tweetId, CancellationToken cancellationToken = default)
    {
        var like = await _context.Set<Like>()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.TweetId == tweetId, cancellationToken);

        if (like == null)
        {
            throw new InvalidOperationException("You have not liked this tweet.");
        }

        _context.Set<Like>().Remove(like);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

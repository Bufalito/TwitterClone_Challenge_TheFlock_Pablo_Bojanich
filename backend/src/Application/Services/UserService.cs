using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class UserService : IUserService
{
    private readonly DbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(DbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validate username uniqueness
        var usernameExists = await _context.Set<User>()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
        }

        // Validate email uniqueness
        var emailExists = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
        }

        // Create user entity
        var user = new User(request.Username, request.Email, request.Name);

        // Hash password
        var hashedPassword = _passwordHasher.HashPassword(user, request.Password);

        // Store hashed password (we'll add this property to User entity)
        SetPasswordHash(user, hashedPassword);

        // Set optional fields if provided
        if (!string.IsNullOrWhiteSpace(request.Bio))
        {
            SetBio(user, request.Bio);
        }

        if (!string.IsNullOrWhiteSpace(request.Avatar))
        {
            SetAvatar(user, request.Avatar);
        }

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    private static void SetPasswordHash(User user, string passwordHash)
    {
        var property = typeof(User).GetProperty("PasswordHash");
        property?.SetValue(user, passwordHash);
    }

    private static void SetBio(User user, string bio)
    {
        var property = typeof(User).GetProperty("Bio");
        property?.SetValue(user, bio);
    }

    private static void SetAvatar(User user, string avatar)
    {
        var property = typeof(User).GetProperty("Avatar");
        property?.SetValue(user, avatar);
    }

    public async Task<UserProfileResponse?> GetByUsernameAsync(string username, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var user = await _context.Set<User>()
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Tweets)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user == null)
        {
            return null;
        }

        var isFollowed = currentUserId.HasValue && user.Followers.Any(f => f.FollowerId == currentUserId.Value);

        return new UserProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.Avatar,
            user.Followers.Count,
            user.Following.Count,
            user.Tweets.Count,
            user.CreatedAtUtc,
            isFollowed
        );
    }

    public async Task<List<UserSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        var users = await _context.Set<User>()
            .Where(u => u.Username.ToLower().Contains(normalizedQuery) ||
                       u.DisplayName.ToLower().Contains(normalizedQuery))
            .Include(u => u.Followers)
            .OrderBy(u => u.Username)
            .Take(20)
            .ToListAsync(cancellationToken);

        return users.Select(u => new UserSearchResult(
            u.Id,
            u.Username,
            u.DisplayName,
            u.Bio,
            u.Avatar,
            u.Followers.Count
        )).ToList();
    }

    public async Task FollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default)
    {
        // Check if users exist
        var followerExists = await _context.Set<User>()
            .AnyAsync(u => u.Id == followerId, cancellationToken);
        
        if (!followerExists)
        {
            throw new InvalidOperationException("Follower user not found.");
        }

        var followedExists = await _context.Set<User>()
            .AnyAsync(u => u.Id == followedId, cancellationToken);
        
        if (!followedExists)
        {
            throw new InvalidOperationException("User to follow not found.");
        }

        // Check if already following
        var alreadyFollowing = await _context.Set<Follow>()
            .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId, cancellationToken);

        if (alreadyFollowing)
        {
            throw new InvalidOperationException("Already following this user.");
        }

        // Create follow relationship
        var follow = new Follow(followerId, followedId);
        _context.Set<Follow>().Add(follow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnfollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default)
    {
        var follow = await _context.Set<Follow>()
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId, cancellationToken);

        if (follow == null)
        {
            throw new InvalidOperationException("Not following this user.");
        }

        _context.Set<Follow>().Remove(follow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserSearchResult>> GetSuggestedUsersAsync(Guid? currentUserId = null, int limit = 3, CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = _context.Set<User>()
            .Include(u => u.Followers);

        // If user is authenticated, exclude users they already follow and themselves
        if (currentUserId.HasValue)
        {
            var followedIds = await _context.Set<Follow>()
                .Where(f => f.FollowerId == currentUserId.Value)
                .Select(f => f.FollowedId)
                .ToListAsync(cancellationToken);

            query = query.Where(u => u.Id != currentUserId.Value && !followedIds.Contains(u.Id));
        }

        // Get users with most followers
        var users = await query
            .OrderByDescending(u => u.Followers.Count)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return users.Select(u => new UserSearchResult(
            u.Id,
            u.Username,
            u.DisplayName,
            u.Bio,
            u.Avatar,
            u.Followers.Count
        )).ToList();
    }
}

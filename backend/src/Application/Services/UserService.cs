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

    public async Task<UserProfileResponse?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
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
            user.CreatedAtUtc
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
}

using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<UserProfileResponse?> GetByUsernameAsync(string username, Guid? currentUserId = null, CancellationToken cancellationToken = default);

    Task<List<UserSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);

    Task FollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default);

    Task UnfollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default);

    Task<List<UserSearchResult>> GetSuggestedUsersAsync(Guid? currentUserId = null, int limit = 3, CancellationToken cancellationToken = default);
}

using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<UserProfileResponse?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<List<UserSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

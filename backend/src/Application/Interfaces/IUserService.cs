using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}

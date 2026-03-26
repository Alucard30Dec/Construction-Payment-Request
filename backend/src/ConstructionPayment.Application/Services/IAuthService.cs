using ConstructionPayment.Application.Dtos.Auth;

namespace ConstructionPayment.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

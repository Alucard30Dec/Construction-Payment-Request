using ConstructionPayment.Application.Dtos.Users;

namespace ConstructionPayment.Application.Dtos.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = new();
}

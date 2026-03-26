using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.Users;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? RoleProfileId { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
}

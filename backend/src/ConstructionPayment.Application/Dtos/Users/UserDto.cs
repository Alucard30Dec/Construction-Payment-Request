using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public Guid? RoleProfileId { get; set; }
    public string? RoleProfileCode { get; set; }
    public string? RoleProfileName { get; set; }
    public IReadOnlyCollection<string> PermissionCodes { get; set; } = Array.Empty<string>();
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

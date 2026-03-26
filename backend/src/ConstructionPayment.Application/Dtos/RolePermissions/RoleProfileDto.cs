namespace ConstructionPayment.Application.Dtos.RolePermissions;

public class RoleProfileDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public IReadOnlyCollection<string> GrantedPermissionCodes { get; set; } = Array.Empty<string>();
}

namespace ConstructionPayment.Application.Dtos.RolePermissions;

public class CreateRoleProfileRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CloneFromRoleProfileId { get; set; }
    public IReadOnlyCollection<string> GrantedPermissionCodes { get; set; } = Array.Empty<string>();
}

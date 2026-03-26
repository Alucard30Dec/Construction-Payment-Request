namespace ConstructionPayment.Application.Dtos.RolePermissions;

public class CurrentUserPermissionDto
{
    public Guid? RoleProfileId { get; set; }
    public string? RoleProfileCode { get; set; }
    public string? RoleProfileName { get; set; }
    public IReadOnlyCollection<string> PermissionCodes { get; set; } = Array.Empty<string>();
}

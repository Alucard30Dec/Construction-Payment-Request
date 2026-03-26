namespace ConstructionPayment.Application.Dtos.RolePermissions;

public class SaveRoleProfilePermissionsRequest
{
    public IReadOnlyCollection<string> GrantedPermissionCodes { get; set; } = Array.Empty<string>();
}

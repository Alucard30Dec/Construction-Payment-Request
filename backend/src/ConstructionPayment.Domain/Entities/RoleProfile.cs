using ConstructionPayment.Domain.Common;

namespace ConstructionPayment.Domain.Entities;

public class RoleProfile : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermissionGrant> PermissionGrants { get; set; } = new List<RolePermissionGrant>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}

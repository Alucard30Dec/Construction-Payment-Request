using ConstructionPayment.Domain.Common;

namespace ConstructionPayment.Domain.Entities;

public class RolePermissionGrant : AuditableEntity
{
    public Guid RoleProfileId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }

    public RoleProfile? RoleProfile { get; set; }
}

using Microsoft.AspNetCore.Authorization;

namespace ConstructionPayment.Infrastructure.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public const string PolicyPrefix = "Permission:";

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    public string PermissionCode { get; }
}

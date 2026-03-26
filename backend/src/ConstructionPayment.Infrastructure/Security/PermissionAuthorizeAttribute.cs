using Microsoft.AspNetCore.Authorization;

namespace ConstructionPayment.Infrastructure.Security;

public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionAuthorizeAttribute(string permissionCode)
    {
        Policy = $"{PermissionRequirement.PolicyPrefix}{permissionCode}";
    }
}

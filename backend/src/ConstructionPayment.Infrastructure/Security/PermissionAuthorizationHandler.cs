using System.Security.Claims;
using ConstructionPayment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ConstructionPayment.Infrastructure.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRolePermissionResolver _rolePermissionResolver;

    public PermissionAuthorizationHandler(IRolePermissionResolver rolePermissionResolver)
    {
        _rolePermissionResolver = rolePermissionResolver;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        var hasPermission = await _rolePermissionResolver.HasPermissionAsync(
            userId,
            requirement.PermissionCode,
            CancellationToken.None);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

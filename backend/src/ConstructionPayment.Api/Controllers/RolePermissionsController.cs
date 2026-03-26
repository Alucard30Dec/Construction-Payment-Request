using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.RolePermissions;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/role-permissions")]
public class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public RolePermissionsController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.GetCurrentUserPermissionsAsync(cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.GetPermissionCatalogAsync(cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpGet("profiles")]
    public async Task<IActionResult> GetProfiles(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.GetRoleProfilesAsync(cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpGet("profiles/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.GetRoleProfileByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpPost("profiles")]
    public async Task<IActionResult> Create([FromBody] CreateRoleProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.CreateRoleProfileAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpPut("profiles/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.UpdateRoleProfileAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpPut("profiles/{id:guid}/permissions")]
    public async Task<IActionResult> SavePermissions(Guid id, [FromBody] SaveRoleProfilePermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.SavePermissionsAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.RoleProfilesManage)]
    [HttpDelete("profiles/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _rolePermissionService.DeleteRoleProfileAsync(id, cancellationToken);
        return NoContent();
    }
}

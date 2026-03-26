using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.Users;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Domain.Enums;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[PermissionAuthorize(PermissionCodes.UsersManage)]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRolePermissionService _rolePermissionService;

    public UsersController(IUserService userService, IRolePermissionService rolePermissionService)
    {
        _userService = userService;
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = Enum.GetValues<UserRole>().Select(x => new { name = x.ToString(), value = x }).ToArray();
        return Ok(roles);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] UserQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("role-profiles")]
    public async Task<IActionResult> GetRoleProfiles(CancellationToken cancellationToken)
    {
        var profiles = await _rolePermissionService.GetRoleProfilesAsync(cancellationToken);
        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _userService.ResetPasswordAsync(id, request, cancellationToken);
        return Ok(new { message = "Đặt lại mật khẩu thành công." });
    }
}
